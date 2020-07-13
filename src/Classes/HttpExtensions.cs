using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JustPlanIt.Models;

namespace JustPlanIt.Classes
{
    public static class HttpExtensions
    {
        private const string JSON_MEDIA_TYPE = "application/json";
        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string requestUri, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, JSON_MEDIA_TYPE);

            return client.PostAsync(requestUri, content);
        }
        public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string requestUri, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, JSON_MEDIA_TYPE);

            return client.PutAsync(requestUri, content);
        }
        public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string requestUri, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, JSON_MEDIA_TYPE);

            return client.PatchAsync(requestUri, content);
        }
        public static T ReadJson<T>(this HttpContent content)
        {
            var result = content.ReadAsStringAsync().Result;
            var jsonObject = JsonSerializer.Deserialize<T>(result, _options);

            return jsonObject;
        }
    }
}