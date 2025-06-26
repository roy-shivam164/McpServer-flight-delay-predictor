using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace McpServer_flight_delay_predictor.Tools;

public class FlightStatusInput
{
    public string FlightNumber { get; set; } = string.Empty;
    public string FlightDate { get; set; } = string.Empty; // Format: yyyy-MM-dd
    public string DepartureAirportCode { get; set; } = string.Empty;
}

[McpServerToolType]
public sealed class FlightDelayCheckerTool
{
    private static readonly HttpClient _httpClient = new();

    [McpServerTool, Description("Checks if a flight is delayed and by how much time using AviationStack API. If user asks for weather of area near departure airport in case of delay, call AirportWeatherTool.")]
    public static async Task<string> CheckFlightDelayAsync(FlightStatusInput input)
    {
        string url = $"https://api.aviationstack.com/v1/flights?access_key=1d3ba558b88344382ce52c64fcdacf02";

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        // The root is now an object, not an array
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return "API response format error: root object not found.";

        if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            return "API response format error: 'data' array not found.";

        foreach (var flight in data.EnumerateArray())
        {
            // Get flight number safely, fallback to 'number' if 'iata' is null
            string? flightNum = null;
            if (flight.TryGetProperty("flight", out var flightObj))
            {
                if (flightObj.TryGetProperty("iata", out var numberProp) && numberProp.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(numberProp.GetString()))
                {
                    flightNum = numberProp.GetString();
                }
                else if (flightObj.TryGetProperty("number", out var fallbackNumberProp) && fallbackNumberProp.ValueKind == JsonValueKind.String)
                {
                    flightNum = fallbackNumberProp.GetString();
                }
            }

            // Get flight date safely
            string? flightDate = null;
            if (flight.TryGetProperty("flight_date", out var dateProp) &&
                dateProp.ValueKind == JsonValueKind.String)
            {
                flightDate = dateProp.GetString();
            }

            // Get departure airport code safely
            string? depIata = null;
            JsonElement depObj = default;
            if (flight.TryGetProperty("departure", out depObj) &&
                depObj.TryGetProperty("iata", out var depIataProp) &&
                depIataProp.ValueKind == JsonValueKind.String)
            {
                depIata = depIataProp.GetString();
            }

            // Match all input fields
            if (flightNum == input.FlightNumber &&
                flightDate == input.FlightDate &&
                depIata == input.DepartureAirportCode)
            {
                int? delay = null;
                if (depObj.TryGetProperty("delay", out var delayProp) && delayProp.ValueKind == JsonValueKind.Number)
                {
                    delay = delayProp.GetInt32();
                }

                if (delay.HasValue && delay.Value > 0)
                {
                    //// Call AirportWeatherTool for weather info
                    //var cityName = await AirportCityPromptTool.GetCityPromptByAirportCodeAsync(new AirportCityInput { DepartureAirportCode = depIata });
                    //var weatherInfo = await AirportCityWeather.GetWeatherByCityNameAsync(new CityInput { CityName = cityName });

                    return $"Your flight {flightNum} on {flightDate} from {depIata} is delayed by {delay.Value} minutes.\n";
                }
                else
                {
                    return $"Your flight {flightNum} on {flightDate} from {depIata} is not delayed.";
                }
            }
        }

        return "No matching flight found for the provided details.";
    }
}