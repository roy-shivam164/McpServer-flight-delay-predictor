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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();
app.UseCors("AllowFrontend");

app.MapMcp();

app.Run();