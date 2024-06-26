using Microsoft.Extensions.Options;
using Tenray.ZoneTree;

namespace YATsDb.Lite;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConfiguration(builder.Configuration);
        builder.Logging.AddZLoggerConfiguration(builder.Configuration);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });


        builder.Services
           .AddOptions<YATsDb.Lite.Services.Configuration.DbSetup>()
           .BindConfiguration("DbSetup");

        builder.Services.AddSingleton<IValidateOptions<YATsDb.Lite.Services.Configuration.DbSetup>, YATsDb.Lite.Services.Configuration.DbSetupValidator>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddMemoryCache();
        builder.Services.AddProblemDetails();

        builder.Services.AddTransient<YATsDb.Core.Services.IManagementService, YATsDb.Core.Services.ManagementService>();
        builder.Services.AddTransient<YATsDb.Core.Services.IDalServices, YATsDb.Core.Services.DalServices>();

        builder.Services.AddTransient<YATsDb.Core.HighLevel.IYatsdbHighLevelStorage, YATsDb.Core.HighLevel.YatsdbHighLevelStorage>();
        builder.Services.AddTransient<YATsDb.Core.LowLevel.IYatsdbLowLevelStorage, YATsDb.Core.LowLevel.YatsdbLowLevelStorage>();
        builder.Services.AddTransient<YATsDb.Core.LowLevel.IKvStorage, YATsDb.Core.LowLevel.KvStorage>();
        builder.Services.AddSingleton<IZoneTree<byte[], byte[]>>(sp =>
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

        builder.Services.AddHostedService<Infrastructure.Workers.ZoneTreeMaintainerHostedService<byte[], byte[]>>();
        builder.Services.AddTransient<YATsDb.Core.Services.ICache, YATsDb.Lite.Services.Implementation.YatsdbCache>();


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

        app.AddAppEndpoints();

        app.MapGet("/", () => "YATsDb.Lite");
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
