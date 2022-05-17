namespace HousingRepairsSchedulingApi;

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public static class SentryHelper
{
    public static void SetupSentry(this IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.UseSentry(o =>
        {
            o.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == Environments.Development)
            {
                o.Debug = true;
                o.TracesSampleRate = 1.0;
            }
        });
    }
}
