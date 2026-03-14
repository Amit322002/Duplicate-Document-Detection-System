using System.Net.Http.Headers;

namespace DuplicateDocsFinder.Service
{
    public class SupabaseStorageService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public SupabaseStorageService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
        {
            var baseUrl = _config["Supabase:Url"];
            var apiKey = _config["Supabase:ApiKey"];
            var bucket = _config["Supabase:Bucket"];

            var url = $"{baseUrl}/storage/v1/object/{bucket}/{fileName}";

            var content = new ByteArrayContent(fileBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PutAsync(url, content);

            response.EnsureSuccessStatusCode();

            return $"{baseUrl}/storage/v1/object/public/{bucket}/{fileName}";
        }
    }
}