// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Services;

/// <summary>
/// HTTP client for integrating with external health data APIs
/// Handles request/response serialization and error handling
/// </summary>
public class HttpHealthDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpHealthDataClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpHealthDataClient(
        HttpClient httpClient,
        ILogger<HttpHealthDataClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Fetch health data from remote API endpoint
    /// </summary>
    public async Task<List<HealthDataRecord>> FetchHealthDataAsync(
        string apiUrl,
        string deviceType,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate:yyyy-MM-dd}");

            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate:yyyy-MM-dd}");

            queryParams.Add($"deviceType={deviceType}");

            var query = string.Join("&", queryParams);
            var url = $"{apiUrl}?{query}";

            _logger.LogInformation("Fetching health data from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var records = JsonSerializer.Deserialize<List<HealthDataRecord>>(content, _jsonOptions);

            _logger.LogInformation("Successfully fetched {Count} records from API", records?.Count ?? 0);

            return records ?? new List<HealthDataRecord>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching health data from: {ApiUrl}", apiUrl);
            throw new HealthDataException("Failed to fetch health data from API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching health data");
            throw new HealthDataException("Unexpected error fetching health data", ex);
        }
    }

    /// <summary>
    /// Upload parsed health data to remote service
    /// </summary>
    public async Task<string> UploadHealthDataAsync(
        string uploadUrl,
        List<HealthDataRecord> records)
    {
        try
        {
            if (records == null || records.Count == 0)
            {
                _logger.LogWarning("No records to upload");
                return string.Empty;
            }

            var payload = new
            {
                Records = records,
                UploadedAt = DateTime.UtcNow,
                RecordCount = records.Count
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Uploading {Count} health records to: {Url}", records.Count, uploadUrl);

            var response = await _httpClient.PostAsync(uploadUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Upload completed successfully. Response: {Response}", responseContent);

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error uploading health data");
            throw new HealthDataException("Failed to upload health data", ex);
        }
    }

    /// <summary>
    /// Check API health and connectivity
    /// </summary>
    public async Task<bool> CheckApiHealthAsync(string healthCheckUrl)
    {
        try
        {
            _logger.LogDebug("Checking API health at: {Url}", healthCheckUrl);

            var response = await _httpClient.GetAsync(healthCheckUrl, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API health check failed");
            return false;
        }
    }

    /// <summary>
    /// Download export file from API
    /// </summary>
    public async Task<string> DownloadExportAsync(string downloadUrl, string outputPath)
    {
        try
        {
            _logger.LogInformation("Downloading export from: {Url}", downloadUrl);

            using (var response = await _httpClient.GetAsync(downloadUrl))
            {
                response.EnsureSuccessStatusCode();

                // Create output directory if needed
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Download to file
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await contentStream.CopyToAsync(fileStream);
                }

                _logger.LogInformation("Export downloaded successfully to: {Path}", outputPath);
                return outputPath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download export");
            throw new HealthDataException("Failed to download export from API", ex);
        }
    }

    /// <summary>
    /// Fetch device information from API
    /// </summary>
    public async Task<Dictionary<string, object>?> GetDeviceInfoAsync(string apiUrl, string deviceId)
    {
        try
        {
            var url = $"{apiUrl}/devices/{deviceId}";
            _logger.LogDebug("Fetching device info from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var deviceInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);

            return deviceInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch device info");
            return null;
        }
    }

    /// <summary>
    /// Set request timeout
    /// </summary>
    public void SetTimeout(TimeSpan timeout)
    {
        _httpClient.Timeout = timeout;
        _logger.LogDebug("HTTP client timeout set to: {Timeout}ms", timeout.TotalMilliseconds);
    }

    /// <summary>
    /// Add custom header to all requests
    /// </summary>
    public void AddDefaultHeader(string name, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(name, value);
        _logger.LogDebug("Added default header: {Name}", name);
    }
}
