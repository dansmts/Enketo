using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enketo.Shared
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureMediatR();
            services.ConfigureFluentValidation();

            return services;
        }

        private static IServiceCollection ConfigureMediatR(this IServiceCollection services)
        {
            services.AddMediatR(typeof(IServiceCollectionExtensions).Assembly);
            return services;
        }

        public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
        {
            services.AddFluentValidation(new[] { typeof(IServiceCollectionExtensions).Assembly });
            return services;
        }

    }
}
