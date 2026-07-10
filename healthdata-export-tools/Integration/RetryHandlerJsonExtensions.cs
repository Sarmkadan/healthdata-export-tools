using System;
using System.Text.Json;

namespace HealthDataExportTools.Integration
{
    public static class RetryHandlerJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string ToJson(this RetryHandler value, bool indented = false)
        {
            if (indented)
            {
                var indentedOptions = new JsonSerializerOptions(Options) { WriteIndented = true };
                return JsonSerializer.Serialize(value, indentedOptions);
            }
            return JsonSerializer.Serialize(value, Options);
        }

        public static RetryHandler? FromJson(string json)
        {
            return JsonSerializer.Deserialize<RetryHandler>(json, Options);
        }

        public static bool TryFromJson(string json, out RetryHandler? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<RetryHandler>(json, Options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
