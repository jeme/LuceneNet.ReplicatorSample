using System;
using System.Text;
using System.Threading.Tasks;
using Lucene.NET.ReplicatorSample.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lucene.NET.ReplicatorSample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<KestrelServerOptions>(options =>
            //{
            //    options.AllowSynchronousIO = true;
            //});

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Index.Instance.Initialize(env);

            DataIngestSimulator simulator = new DataIngestSimulator();
            simulator.Start();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/api/replicate/{*url}", async context =>
                {
                    await Task.Yield();
                    Index.Instance.ReplicatorService.Perform(context.Request, context.Response);
                });
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("UP!");
                });
            });
        }
    }
}
