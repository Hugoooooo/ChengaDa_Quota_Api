using ChengDaApi.DBRepositories.ImplRepositories;
using ChengDaApi.DBRepositories.IRepositories;
using ChengDaApi.Services;
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
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IPunchDetailRepository, PunchDetailRepository>();
            services.AddScoped<IDayoffDetailRepository, DayoffDetailRepository>();
            services.AddScoped<IQuoteDetailRepository, QuoteDetailRepository>();
            services.AddScoped<ISystemParameterRepository, SystemParameterRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IPurchaseDetailRepository, PurchaseDetailRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IShipOrderRepository, ShipOrderRepository>();
            services.AddScoped<IShipDetailRepository, ShipDetailRepository>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddScoped<JwtTokenService>();
            return services;
        }
    }
}
