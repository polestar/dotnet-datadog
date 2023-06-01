using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
using Datadog.Trace;
using Datadog.Trace.Annotations;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace DotnetDatadog.Example2;

public class Function
{
  [Trace]
  [Logging(LogEvent = true)]
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    Request request,
    ILambdaContext context)
  {
    await Think();

    var response = new
    {
      Data = "Japp",
    };

    return new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = (int)HttpStatusCode.OK,
      Body = JsonSerializer.Serialize(response),
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
  }

  [Trace(OperationName = "example2.do-heavy-thinking", ResourceName = "Example2.Think")]
  private async Task Think()
  {
    using (var scope = Tracer.Instance.StartActive("example2.do-heavy-thinking-in-scope"))
    {
      scope.Span.ResourceName = "Example2.ThinkInScope";
      var random = new Random();
      var thinkingTime = random.Next(50, 1000);

      scope.Span.SetTag("thinking.time", $"{thinkingTime}ms");

      Logger.LogInformation($"Need to think for {thinkingTime}ms");

      await Task.Delay(thinkingTime);
    }
  }
}