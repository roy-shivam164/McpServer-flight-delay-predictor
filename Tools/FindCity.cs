using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;

namespace McpServer_flight_delay_predictor.Tools;

public class AirportCityInput
{
    public string DepartureAirportCode { get; set; } = string.Empty;
}

[McpServerToolType]
public sealed class AirportCityPromptTool
{
    [McpServerTool, Description("find the city near the given departure airport")]
    public static Task<string> GetCityPromptByAirportCodeAsync(AirportCityInput input)
    {
        if (string.IsNullOrWhiteSpace(input.DepartureAirportCode))
            return Task.FromResult("No airport code provided.");

        string prompt = $"Which city is near the airport with IATA code '{input.DepartureAirportCode}'?";
        return Task.FromResult(prompt);
    }
}