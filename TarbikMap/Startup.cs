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
                return new CompositeTaskSource()
                    .Add("sw", new StreetviewTaskSource(serviceProvider.GetRequiredService<IDownloader>(), serviceProvider.GetRequiredService<EnvironmentConfig>(), false, false))
                    .Add("fl", new FlickrTaskSource(serviceProvider.GetRequiredService<IDownloader>(), serviceProvider.GetRequiredService<EnvironmentConfig>()))
                    .Add("oc", new OpenStreetCamTaskSource(serviceProvider.GetRequiredService<IDownloader>()))
                    .Add("wd", new WikidataTaskSource())
                    ;
            });

            services.AddSingleton<IAreaSource>((serviceProvider) =>
            {
                return new CachingAreaSource(new CompositeAreaSource()
                    .Add("co", new CommonAreaSource())
                    .Add("ne", new NaturalEarthAreaSource())
                    .Add("os", new OsmAreaSource(serviceProvider.GetRequiredService<IDownloader>())));
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
