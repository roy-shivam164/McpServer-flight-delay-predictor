using McpServer_flight_delay_predictor.Tools;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
//using TestServerWithHosting.Tools;
//using TestServerWithHosting.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<JsonPlaceholderTool>()
    .WithTools<SampleLlmTool>()
    .WithTools<FlightDelayCheckerTool>()
    .WithTools<AirportCityWeather>()
    .WithTools<AirportCityPromptTool>();
    //.WithResources<SimpleResourceType>();

//builder.Services.AddOpenTelemetry()
//    .WithTracing(b => b.AddSource("*")
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation())
//    .WithMetrics(b => b.AddMeter("*")
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation())
//    .WithLogging()
//    .UseOtlpExporter();

var app = builder.Build();

app.MapMcp();

app.Run();