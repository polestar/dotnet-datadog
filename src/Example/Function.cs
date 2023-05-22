using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
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
  public override Task<Result> Handler(
    Request request,
    ILambdaContext context)
  {
    return Task.FromResult(new Result
    {
      Name = "Testing",
    });
  }
}