using DuplicateDocsFinder.Dto;
using System.Net.Http.Json;

namespace DuplicateDocsFinder.Service
{
    public interface IVectorService
    {
        Task InsertVectorAsync(string id, float[] vector);

        Task<List<Guid>> SearchSimilarAsync(float[] vector);
    }

    public class VectorService : IVectorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _qdrantUrl;
        private readonly string _collection;

        public VectorService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _qdrantUrl = configuration["Qdrant:Url"];
            _collection = configuration["Qdrant:Collection"];
        }

        public async Task InsertVectorAsync(string id, float[] vector)
        {
            try
            {
                var body = new
                {
                    points = new[]
                    {
                        new
                        {
                            id = id,
                            vector = vector
                        }
                    }
                };

                var url = $"{_qdrantUrl}/collections/{_collection}/points";

                await _httpClient.PutAsJsonAsync(url, body);
            }
            catch
            {
                // intentionally ignored
            }
        }

        public async Task<List<Guid>> SearchSimilarAsync(float[] vector)
        {
            try
            {
                var body = new
                {
                    vector = vector,
                    limit = 20
                };

                var url = $"{_qdrantUrl}/collections/{_collection}/points/search";

                var response = await _httpClient.PostAsJsonAsync(url, body);

                if (!response.IsSuccessStatusCode)
                    return new List<Guid>();

                var result = await response.Content.ReadFromJsonAsync<QdrantResponse>();

                if (result?.result == null)
                    return new List<Guid>();

                return result.result
                    .Where(x => !string.IsNullOrEmpty(x.id))
                    .Select(x => Guid.Parse(x.id))
                    .ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }
    }

    public class QdrantResponse
    {
        public List<QdrantPoint> result { get; set; }
    }

    public class QdrantPoint
    {
        public string id { get; set; }
        public float score { get; set; }
    }
}