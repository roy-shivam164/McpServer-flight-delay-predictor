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

    [McpServerTool, Description("Checks if a flight is delayed and by how much time using AviationStack API.")]
    public static async Task<string> CheckFlightDelayAsync(FlightStatusInput input)
    {
        string url = $"https://685babc389952852c2da7875.mockapi.io/check";

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        // The root is an array
        if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
            return "API response format error: root array not found or empty.";

        var firstObj = doc.RootElement[0];
        if (!firstObj.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            return "API response format error: 'data' array not found.";

        foreach (var flight in data.EnumerateArray())
        {
            // Get flight number safely
            string? flightNum = null;
            if (flight.TryGetProperty("flight", out var flightObj) &&
                flightObj.TryGetProperty("iata", out var numberProp) &&
                numberProp.ValueKind == JsonValueKind.String)
            {
                flightNum = numberProp.GetString();
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
            if (flight.TryGetProperty("departure", out var depObj) &&
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
                    return $"Your flight {flightNum} on {flightDate} from {depIata} is delayed by {delay.Value} minutes.";
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