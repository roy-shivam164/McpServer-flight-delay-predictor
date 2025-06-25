using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace McpServer_flight_delay_predictor.Tools;

public class WeatherInput
{
    public string DepartureAirportCode { get; set; } = string.Empty;
}

[McpServerToolType]
public sealed class AirportWeatherTool
{
    private static readonly HttpClient _httpClient = new();

    [McpServerTool, Description("Gets the current weather for the given departure airport code.")]
    public static async Task<string> GetWeatherByAirportCodeAsync(WeatherInput input)
    {
        string url = $"https://685babc389952852c2da7875.mockapi.io/weather";
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        // The root is an array with a single object mapping airport codes to weather strings
        if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
            return "Weather API response format error: root array not found or empty.";

        var weatherObj = doc.RootElement[0];
        if (weatherObj.TryGetProperty(input.DepartureAirportCode, out var weatherProp) && weatherProp.ValueKind == JsonValueKind.String)
        {
            return $"Weather at {input.DepartureAirportCode}: {weatherProp.GetString()}";
        }
        else
        {
            return $"Weather information not available for airport code: {input.DepartureAirportCode}";
        }
    }
}