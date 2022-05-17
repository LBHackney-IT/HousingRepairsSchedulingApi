using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;

namespace HousingRepairsSchedulingApi
{
    [ExcludeFromCodeCoverage]
    public class LambdaEntryPoint : APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.SetupSentry(true);
            builder
                .UseStartup<Startup>();
        }
    }
}
