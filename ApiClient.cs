using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Serilog;

namespace TestFramework
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl, TimeSpan? timeout = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            Log.Information("API Client initialized for base URL: {BaseUrl}", _baseUrl);
        }

        // Authentication methods
        public void SetBearerToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            Log.Information("Bearer token set for API client");
        }

        public void SetBasicAuth(string username, string password)
        {
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);
            Log.Information("Basic authentication set for user: {Username}", username);
        }

        public void SetApiKey(string keyName, string keyValue, bool inHeader = true)
        {
            if (inHeader)
            {
                _httpClient.DefaultRequestHeaders.Add(keyName, keyValue);
                Log.Information("API key set in header: {KeyName}", keyName);
            }
        }

        // GET requests
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            var url = BuildUrl(endpoint, queryParams);
            Log.Information("Making GET request to: {Url}", url);

            try
            {
                var response = await _httpClient.GetAsync(url);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during GET request to {Url}", url);
                throw;
            }
        }

        // POST requests
        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? payload = null)
        {
            var url = BuildUrl(endpoint);
            Log.Information("Making POST request to: {Url}", url);

            try
            {
                var content = CreateJsonContent(payload);
                var response = await _httpClient.PostAsync(url, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during POST request to {Url}", url);
                throw;
            }
        }

        // PUT requests
        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object payload)
        {
            var url = BuildUrl(endpoint);
            Log.Information("Making PUT request to: {Url}", url);

            try
            {
                var content = CreateJsonContent(payload);
                var response = await _httpClient.PutAsync(url, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during PUT request to {Url}", url);
                throw;
            }
        }

        // DELETE requests
        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            var url = BuildUrl(endpoint);
            Log.Information("Making DELETE request to: {Url}", url);

            try
            {
                var response = await _httpClient.DeleteAsync(url);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during DELETE request to {Url}", url);
                throw;
            }
        }

        // PATCH requests
        public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object payload)
        {
            var url = BuildUrl(endpoint);
            Log.Information("Making PATCH request to: {Url}", url);

            try
            {
                var content = CreateJsonContent(payload);
                var response = await _httpClient.PatchAsync(url, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during PATCH request to {Url}", url);
                throw;
            }
        }

        // Helper methods
        private string BuildUrl(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            var url = endpoint.StartsWith('/') ? endpoint.Substring(1) : endpoint;
            
            if (queryParams != null && queryParams.Any())
            {
                var queryString = string.Join("&", 
                    queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{queryString}";
            }

            return url;
        }

        private StringContent CreateJsonContent(object? payload)
        {
            if (payload == null) return new StringContent(string.Empty, Encoding.UTF8, "application/json");
            
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            Log.Debug("Request payload: {Payload}", json);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            Log.Information("API Response - Status: {StatusCode}, Content Length: {ContentLength}", 
                response.StatusCode, content?.Length ?? 0);

            var apiResponse = new ApiResponse<T>
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                RawContent = content
            };

            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
            {
                try
                {
                    apiResponse.Data = JsonConvert.DeserializeObject<T>(content);
                }
                catch (JsonException ex)
                {
                    Log.Warning(ex, "Failed to deserialize response content to type {Type}", typeof(T).Name);
                    apiResponse.ErrorMessage = $"Failed to deserialize response: {ex.Message}";
                }
            }
            else
            {
                apiResponse.ErrorMessage = $"API call failed with status {response.StatusCode}: {content}";
                Log.Warning("API call failed: {ErrorMessage}", apiResponse.ErrorMessage);
            }

            return apiResponse;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            Log.Information("API Client disposed");
        }
    }

    public class ApiResponse<T>
    {
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? RawContent { get; set; }
    }
} 