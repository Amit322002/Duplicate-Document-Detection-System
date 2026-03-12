namespace DuplicateDocsFinder.Dto
{
    public class UploadDocumentDto
    {
        public int UserId { get; set; }

        public IFormFile File { get; set; }
    }
}
