using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Internal;

namespace Test
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
            services.AddControllers();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/profiler")
                {
                    await context.Response.WriteAsync(profilerIndexValue.Value, Encoding.UTF8);

                    return;
                }

                await next();
            });

            app.UseMiniProfiler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static readonly Lazy<string> profilerIndexValue = new Lazy<string>(profilerIndex);

        private static string profilerIndex()
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<title>MiniProfiler Index</title>");
            sb.AppendLine($"<link href=\"/profiler/includes.min.css?v={MiniProfilerBaseOptions.Version}\" rel=\"stylesheet\" />");
            sb.AppendLine("<body>");
            sb.AppendLine("<h2><a href='/profiler/results-index'>List of all tracings (auto-refresh)</a></h2>");
            sb.AppendLine("<h2><a href='/profiler/results'>Current/last request</a></h2>");
            sb.AppendLine("<h4><a href='/profiler/results-list'>List of all requests as json</a></h4>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
