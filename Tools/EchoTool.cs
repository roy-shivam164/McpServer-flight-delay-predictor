using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace McpServer_flight_delay_predictor.Tools;

[McpServerToolType]
public sealed class JsonPlaceholderTool
{
    private static readonly HttpClient _httpClient = new(); // Simplified 'new' expression  

    [McpServerTool, Description("Fetches a post from JSON Placeholder by ID.")]
    public static async Task<string> GetPostByIdAsync(string id)
    {
        string url = $"https://jsonplaceholder.typicode.com/posts/{id}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
