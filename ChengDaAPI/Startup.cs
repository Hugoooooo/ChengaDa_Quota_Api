using ChengDaApi.Controllers.Job;
using ChengDaApi.DBRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
            {
                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            }));

            services.AddDbContextPool<CoreDbContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DB")).EnableSensitiveDataLogging());


            services.ConfigureRepositories();
            //向DI容器註冊Quartz服務
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            //向DI容器註冊Job
            services.AddSingleton<TestJob>();

            //向DI容器註冊JobSchedule
            //services.AddSingleton(new JobSchedule(jobName: "111", jobType: typeof(TestJob), cronExpression: "0/30 * * * * ?"));
            //services.AddSingleton(new JobSchedule(jobName: "222", jobType: typeof(TestJob), cronExpression: "0/52 * * * * ?"));

            //向DI容器註冊Host服務
            services.AddSingleton<QuartzHostedService>();
            //services.AddHostedService(provider => provider.GetService<QuartzHostedService>());

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChengDaApi v1"));
            }

            app.UseCors("ApiCorsPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
