using DocumentFormat.OpenXml.Packaging;
using DuplicateDocsFinder.Data;
using DuplicateDocsFinder.Dto;
using DuplicateDocsFinder.Entity;
using DuplicateDocsFinder.ServiceResult;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Security.Cryptography;

namespace DuplicateDocsFinder.Service
{
    public interface IDocumentService
    {
        Task<ServiceResult<string>> UploadDocumentAsync(UploadDocumentDto dto);
    }

    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly VectorService _vectorService;
        private readonly EmbeddingService _embeddingService;

        public DocumentService(
            AppDbContext context,
            IConfiguration configuration,
            VectorService vectorService,
            EmbeddingService embeddingService)
        {
            _context = context;
            _configuration = configuration;
            _vectorService = vectorService;
            _embeddingService = embeddingService;
        }

        public async Task<ServiceResult<string>> UploadDocumentAsync(UploadDocumentDto dto)
        {
            try
            {
                if (dto.File == null || dto.File.Length == 0)
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "File is empty"
                    };
                }

                byte[] fileBytes;

                using (var ms = new MemoryStream())
                {
                    await dto.File.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                // Exact duplicate check
                var fileHash = GenerateSHA256(fileBytes);

                if (await _context.Documents.AnyAsync(x => x.FileHash == fileHash))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        StatusCode = 409,
                        Message = "Exact document already uploaded"
                    };
                }

                var imageBytes = ConvertFileToImage(fileBytes, dto.File.FileName);

                float[] embedding = null;
                List<Guid> candidateIds = new();

                if (imageBytes != null)
                {
                    embedding = _embeddingService.GenerateEmbedding(imageBytes);

                    if (embedding != null)
                    {
                        candidateIds = await _vectorService.SearchSimilarAsync(embedding);

                        var existingDocs = await _context.Documents
                            .Where(x => candidateIds.Contains(x.Id))
                            .ToListAsync();

                        foreach (var doc in existingDocs)
                        {
                            if (!File.Exists(doc.FilePath))
                                continue;

                            var existingBytes = await File.ReadAllBytesAsync(doc.FilePath);

                            var existingImage = ConvertFileToImage(existingBytes, doc.FileName);

                            if (existingImage == null)
                                continue;

                            if (AreImagesSimilar(imageBytes, existingImage))
                            {
                                return new ServiceResult<string>
                                {
                                    Success = false,
                                    StatusCode = 409,
                                    Message = "Same document already uploaded"
                                };
                            }
                        }
                    }
                }

                var basePath = _configuration["DocumentSettings:StoragePath"];
                var userFolder = Path.Combine(basePath, dto.UserId.ToString());

                if (!Directory.Exists(userFolder))
                    Directory.CreateDirectory(userFolder);

                var extension = Path.GetExtension(dto.File.FileName).ToLower();

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(userFolder, fileName);

                await File.WriteAllBytesAsync(filePath, fileBytes);

                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    FileName = fileName,
                    FilePath = filePath,
                    FileHash = fileHash,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                if (embedding != null)
                    await _vectorService.InsertVectorAsync(document.Id.ToString(), embedding);

                return new ServiceResult<string>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Document uploaded successfully"
                };
            }
            catch
            {
                return new ServiceResult<string>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "An error occurred while uploading the document"
                };
            }
        }

        private string GenerateSHA256(byte[] fileBytes)
        {
            using var sha = SHA256.Create();

            var hashBytes = sha.ComputeHash(fileBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        private bool AreImagesSimilar(byte[] img1Bytes, byte[] img2Bytes)
        {
            using var img1 = Cv2.ImDecode(img1Bytes, ImreadModes.Grayscale);
            using var img2 = Cv2.ImDecode(img2Bytes, ImreadModes.Grayscale);

            if (img1.Empty() || img2.Empty())
                return false;

            Cv2.Resize(img1, img1, new Size(900, 900));
            Cv2.Resize(img2, img2, new Size(900, 900));

            var orb = ORB.Create(3000);

            KeyPoint[] kp1;
            KeyPoint[] kp2;

            Mat desc1 = new Mat();
            Mat desc2 = new Mat();

            orb.DetectAndCompute(img1, null, out kp1, desc1);
            orb.DetectAndCompute(img2, null, out kp2, desc2);

            if (desc1.Empty() || desc2.Empty())
                return false;

            var matcher = new BFMatcher(NormTypes.Hamming);
            var matches = matcher.KnnMatch(desc1, desc2, 2);

            int goodMatches = matches.Count(m =>
                m.Length >= 2 && m[0].Distance < 0.75 * m[1].Distance);

            double ratio = (double)goodMatches / matches.Length;

            return goodMatches > 60 && ratio > 0.18;
        }

        private byte[] ConvertPdfFirstPageToImage(byte[] pdfBytes)
        {
            using var stream = new MemoryStream(pdfBytes);

            var pdf = PdfiumViewer.PdfDocument.Load(stream);

            var image = pdf.Render(0, 300, 300, true);

            using var ms = new MemoryStream();

            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();
        }

        private byte[] ConvertDocxToImage(byte[] docxBytes)
        {
            using var ms = new MemoryStream(docxBytes);

            using var doc = WordprocessingDocument.Open(ms, false);

            var imagePart = doc.MainDocumentPart.ImageParts.FirstOrDefault();

            if (imagePart != null)
            {
                using var stream = imagePart.GetStream();
                using var mem = new MemoryStream();
                stream.CopyTo(mem);
                return mem.ToArray();
            }

            return null;
        }

        private byte[] ConvertFileToImage(byte[] fileBytes, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                return fileBytes;

            if (extension == ".pdf")
                return ConvertPdfFirstPageToImage(fileBytes);

            if (extension == ".docx")
                return ConvertDocxToImage(fileBytes);

            return null;
        }
    }
}