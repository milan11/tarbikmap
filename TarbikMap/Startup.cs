namespace TarbikMap
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;
    using TarbikMap.AreaSources;
    using TarbikMap.Common.Downloader;
    using TarbikMap.Hubs;
    using TarbikMap.Storage;
    using TarbikMap.TaskSources;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Startup")]
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new CustomControllerFeatureProvider());
            });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddSignalR();

            services.AddSingleton<EnvironmentConfig>();

            services.AddSingleton<IDownloader>((serviceProvider) => Factories.CreateDownloader(serviceProvider.GetRequiredService<EnvironmentConfig>()));

            services.AddSingleton<StorageMain>();

            services.AddSingleton<GameStateGetter>();

            services.AddSingleton<ITaskSource>((serviceProvider) =>
            {
                var downloader = serviceProvider.GetRequiredService<IDownloader>();
                var environmentConfig = serviceProvider.GetRequiredService<EnvironmentConfig>();

                return new CompositeTaskSource()
                    .Add("sw", new StreetviewTaskSource(downloader, environmentConfig, false, false))
                    .Add("fl", new FlickrTaskSource(downloader, environmentConfig))
                    .Add("oc", new OpenStreetCamTaskSource(downloader))
                    .Add("wd", new WikidataTaskSource(downloader))
                    ;
            });

            services.AddSingleton<IAreaSource>((serviceProvider) =>
            {
                var downloader = serviceProvider.GetRequiredService<IDownloader>();

                return new CachingAreaSource(new CompositeAreaSource()
                    .Add("co", new CommonAreaSource())
                    .Add("ne", new NaturalEarthAreaSource())
                    .Add("os", new OsmAreaSource(downloader)));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GameHub>("/gamestate");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions()
                {
                    OnPrepareResponse = ctx =>
                    {
                        var headers = ctx.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(0),
                        };
                    },
                };

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
