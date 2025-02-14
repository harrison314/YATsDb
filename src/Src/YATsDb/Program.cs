using Microsoft.Extensions.Options;
using NCronJob;
using Serilog;
using Tenray.ZoneTree;
using YATsDb.Components;

namespace YATsDb;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
             .ReadFrom.Configuration(context.Configuration)
             .ReadFrom.Services(services)
             .Enrich.FromLogContext());

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient();

        builder.Services.AddOpenApiDocument();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddValidators();
        builder.Services.AddProblemDetails();

        builder.Services
            .AddOptions<YATsDb.Services.Configuration.DbSetup>()
            .BindConfiguration("DbSetup")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddOptions<YATsDb.Services.Configuration.JsEngineSetup>()
            .BindConfiguration("JsEngineSetup")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddTransient<YATsDb.Core.Services.IManagementService, YATsDb.Core.Services.ManagementService> ();
        builder.Services.AddTransient< YATsDb.Core.Services.IDalServices, YATsDb.Core.Services.DalServices> ();
        builder.Services.AddTransient<Services.Contracts.ICronManagement, Services.Implementation.CronManagement>();
        builder.Services.AddTransient<Services.Contracts.IJsInternalEngine, Services.Implementation.JsEngine.JsInternalEngine>();

        builder.Services.AddTransient< YATsDb.Core.HighLevel.IYatsdbHighLevelStorage, YATsDb.Core.HighLevel.YatsdbHighLevelStorage> ();
        builder.Services.AddTransient< YATsDb.Core.LowLevel.IYatsdbLowLevelStorage, YATsDb.Core.LowLevel.YatsdbLowLevelStorage > ();
        builder.Services.AddTransient<YATsDb.Core.LowLevel.IKvStorage, YATsDb.Core.LowLevel.KvStorage>();
        builder.Services.AddSingleton<IZoneTree<Memory<byte>, Memory<byte>>>(sp =>
        {
            Services.Configuration.DbSetup dbSetup = sp.GetRequiredService<IOptions<Services.Configuration.DbSetup>>().Value;

            return YATsDb.Core.ZoneTreeFactory.Build(factory =>
            {
                factory.SetDataDirectory(dbSetup.DbPath);
                factory.ConfigureWriteAheadLogOptions(opt =>
                {
                    opt.WriteAheadLogMode = dbSetup.WriteAheadLogMode;
                });

                if (dbSetup.MaxMuttableSegmetsCount.HasValue)
                {
                    factory.SetMutableSegmentMaxItemCount(dbSetup.MaxMuttableSegmetsCount.Value);
                }
            });
        });

        builder.Services.AddHostedService<Infrastructure.Workers.ZoneTreeMaintainerHostedService<Memory<byte>, Memory<byte>>>();
        builder.Services.AddTransient<YATsDb.Core.Services.ICache, YATsDb.Services.Implementation.YatsdbCache>();

        builder.Services.AddNCronJob(cfg =>
        {
            cfg.AddJob<Services.Implementation.CronTriggerJob>();
            cfg.AddJob<Services.Implementation.CronStartupRegisterJob>().RunAtStartup();
        });


        WebApplication app = builder.Build();
        UseBasePath(app);

        app.UseForwardedHeaders();

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                await Results.Problem()
                     .ExecuteAsync(context);
            });
        });

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
           // app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
           // app.UseHsts();    
        }
        else
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.UseHttpsRedirection();

        app.AddAppEndpoints();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        app.Run();
    }

    private static void UseBasePath(WebApplication app)
    {
        string? basePath = app.Configuration.GetValue<string>("AppBasePath");
        if (!string.IsNullOrEmpty(basePath))
        {
            app.UsePathBase(basePath);
            app.Logger.LogDebug("Start with base path {basePath}.", basePath);
        }
    }
}
