using ChengDaApi.DBRepositories.ImplRepositories;
using ChengDaApi.DBRepositories.IRepositories;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            services.AddScoped<IDatabase, CoreSrvDb>();
            services.AddScoped<IDapperr, Dapperr>();
            services.AddScoped<IQuoteRepository, QuoteRepository>();
            services.AddScoped<IQuoteDetailRepository, QuoteDetailRepository>();
            services.AddScoped<ISystemParameterRepository, SystemParameterRepository>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            return services;
        }
    }
}
