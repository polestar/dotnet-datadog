using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetDatadog.Shared;

public abstract class FunctionBase<TRequest, TResponse>
{
  protected readonly IServiceProvider ServiceProvider;
  protected readonly IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
  private readonly APIGatewayHttpApiV2ProxyResponse _noBodyResponse = new APIGatewayHttpApiV2ProxyResponse
  {
    Body = JsonSerializer.Serialize(new[]
    {
      new
      {
        Key = "Body",
        Message = "Invalid Request"
      }
    }),
    StatusCode = (int)HttpStatusCode.BadRequest,
  };

  public FunctionBase() => ServiceProvider = BuildServiceProvider();

  protected virtual void ConfigureServices(IServiceCollection services) { }

  private IServiceProvider BuildServiceProvider()
  {
    var services = new ServiceCollection();

    AWSSDKHandler.RegisterXRayForAllServices();
#if DEBUG
    AWSXRayRecorder.Instance.XRayOptions.IsXRayTracingDisabled = true;
#endif

    ConfigureServices(services);

    return services.BuildServiceProvider();
  }

  public abstract Task<TResponse> Handler(
    TRequest request, ILambdaContext context);

  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayProxyRequest request, ILambdaContext context)
  {
    AppendLookup(request);

    if (request.Body == default)
    {
      return _noBodyResponse;
    }

    var parsedRequest = JsonSerializer.Deserialize<TRequest>(
      request.Body,
      new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
      });

    if (parsedRequest == null)
    {
      return _noBodyResponse;
    }

    var response = await Handler(parsedRequest, context);

    Logger.LogInformation(new
    {
      Response = response,
    });

    return new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = (int)HttpStatusCode.OK,
      Body = JsonSerializer.Serialize(response),
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
  }

  private void AppendLookup(APIGatewayProxyRequest apiGatewayProxyRequest)
  {
    try
    {
      var requestContextRequestId = apiGatewayProxyRequest.RequestContext.RequestId;
      var lookupInfo = new Dictionary<string, object>()
      {
        { "LookupInfo", new Dictionary<string, object>{{ "LookupId", requestContextRequestId }} },
      };
      Logger.AppendKeys(lookupInfo);
    }
    catch(Exception) {}
  }
}