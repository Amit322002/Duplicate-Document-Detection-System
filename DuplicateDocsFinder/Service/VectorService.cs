using System.Net.Http.Json;
using System.Text.Json;

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
        private readonly string _apiKey;

        public VectorService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _qdrantUrl = configuration["Qdrant:Url"];
            _collection = configuration["Qdrant:Collection"];
            _apiKey = configuration["Qdrant:ApiKey"];

            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        private async Task EnsureCollectionExistsAsync()
        {
            var checkResponse = await _httpClient.GetAsync($"{_qdrantUrl}/collections/{_collection}");

            if (!checkResponse.IsSuccessStatusCode)
            {
                var body = new
                {
                    vectors = new
                    {
                        size = 1024,
                        distance = "Cosine"
                    }
                };

                await _httpClient.PutAsJsonAsync($"{_qdrantUrl}/collections/{_collection}", body);
            }
        }

        public async Task InsertVectorAsync(string id, float[] vector)
        {
            await EnsureCollectionExistsAsync();

            var payload = new
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

            await _httpClient.PutAsJsonAsync(
                $"{_qdrantUrl}/collections/{_collection}/points",
                payload
            );
        }

        public async Task<List<Guid>> SearchSimilarAsync(float[] vector)
        {
            await EnsureCollectionExistsAsync();

            var body = new
            {
                vector = vector,
                limit = 20
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_qdrantUrl}/collections/{_collection}/points/search",
                body
            );

            if (!response.IsSuccessStatusCode)
                return new List<Guid>();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            var results = new List<Guid>();

            if (json.TryGetProperty("result", out var resultArray))
            {
                foreach (var item in resultArray.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetString();

                    if (Guid.TryParse(id, out var guid))
                        results.Add(guid);
                }
            }

            return results;
        }
    }
}