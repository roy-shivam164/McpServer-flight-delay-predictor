using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace McpServer_flight_delay_predictor.Tools;

public class CityInput
{
    public string CityName { get; set; } = string.Empty;
}

[McpServerToolType]
public sealed class AirportCityWeather
{
    private static readonly HttpClient _httpClient = new();

    [McpServerTool, Description("Gets the current weather for the given city name using OpenWeatherMap.")]
    public static async Task<string> GetWeatherByCityNameAsync(CityInput input)
    {
        if (string.IsNullOrWhiteSpace(input.CityName))
            return "No city name provided.";

        string apiKey = "544be925118cf9c775bbf8b3b08b67f0"; // Replace with your actual API key
        string url = $"https://api.openweathermap.org/data/2.5/weather?appid={apiKey}&q={input.CityName}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return $"Failed to fetch weather for city: {input.CityName} (code: {response.StatusCode})";

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Extract weather description and temperature (in Celsius)
        string? weatherDesc = null;
        double? tempK = null;
        if (doc.RootElement.TryGetProperty("weather", out var weatherArr) &&
            weatherArr.ValueKind == JsonValueKind.Array &&
            weatherArr.GetArrayLength() > 0)
        {
            var weatherObj = weatherArr[0];
            if (weatherObj.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String)
                weatherDesc = descProp.GetString();
        }
        if (doc.RootElement.TryGetProperty("main", out var mainObj) &&
            mainObj.TryGetProperty("temp", out var tempProp) &&
            tempProp.ValueKind == JsonValueKind.Number)
        {
            tempK = tempProp.GetDouble();
        }

        if (weatherDesc != null && tempK.HasValue)
        {
            double tempC = tempK.Value - 273.15;
            return $"Weather in {input.CityName}: {weatherDesc}, {tempC:F1}°C";
        }
        else
        {
            return $"Weather information not available for city: {input.CityName}";
        }
    }
}