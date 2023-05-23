using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
using Datadog.Trace;
using Datadog.Trace.Annotations;
using DotnetDatadog.Shared;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace DotnetDatadog.Example;

public class Function : FunctionBase<Request, Result>
{
  protected override void ConfigureServices(IServiceCollection services)
  {
  }

  [Logging(LogEvent = true)]
  public override async Task<Result> Handler(
    Request request,
    ILambdaContext context)
  {
    await Think();

    return new Result
    {
      Name = "Testing",
    };
  }

  [Trace(OperationName = "example.do-heavy-thinking", ResourceName = "Example.Think")]
  private async Task Think()
  {
    using (var scope = Tracer.Instance.StartActive("example.do-heavy-thinking-in-scope"))
    {
      scope.Span.ResourceName = "Example.ThinkInScope";
      var random = new Random();
      var thinkingTime = random.Next(50, 1000);

      scope.Span.SetTag("thinking.time", $"{thinkingTime}ms");

      Logger.LogInformation($"Need to think for {thinkingTime}ms");

      await Task.Delay(thinkingTime);
    }
  }
}