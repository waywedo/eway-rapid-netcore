using System;
using Eway.Rapid;
using Eway.Rapid.Abstractions.Interfaces;
using Microsoft.Extensions.Options;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class RapidClientExtensions
    {
        /// <summary>
        /// Add Rapid
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        public static IHttpClientBuilder AddRapidClient(this IServiceCollection services, Action<RapidOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions<RapidOptions>().Configure(configureOptions);

            return services.AddHttpClient<IRapidClient, RapidClient>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptionsMonitor<RapidOptions>>().CurrentValue;
                    options.ConfigureHttpClient(client);
                });
        }
    }
}
