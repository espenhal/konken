using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Services;
using web.Settings;

namespace web.Configuration
{
    public static class FplServiceConfiguration
    {
        public static IServiceCollection AddFplService(this IServiceCollection services, AppSettings settings)
        {
            var serviceUrl = settings.FplApiUrl;
            if (!serviceUrl.EndsWith("/"))
                serviceUrl = $"{serviceUrl}/";
            services.AddHttpClient(name: HttpClients.FplApi, client => { client.BaseAddress = new Uri(serviceUrl, UriKind.RelativeOrAbsolute); });

            services.AddSingleton<IFplApiWrapper>(c =>
                new FplApiWrapper(c.GetRequiredService<IHttpClientFactory>(), settings.LeagueId));

            return services;
        }
    }
}