using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
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
    var random = new Random();
    await Task.Delay(random.Next(50, 1000));
  }
}