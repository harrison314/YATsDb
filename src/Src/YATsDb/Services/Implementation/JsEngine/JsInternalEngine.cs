using Jint;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Options;
using Polly;
using YATsDb.Core.Services;
using YATsDb.Services.Configuration;
using YATsDb.Services.Contracts;

namespace YATsDb.Services.Implementation.JsEngine;

public class JsInternalEngine : IJsInternalEngine
{
    private readonly ILoggerFactory loggerFactory;
    private readonly IDalServices dalServices;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IOptions<JsEngineSetup> jsEngineSetup;
    private readonly ILogger<JsInternalEngine> logger;

    public JsInternalEngine(ILoggerFactory loggerFactory,
        IDalServices dalServices,
        IHttpClientFactory httpClientFactory,
        IOptions<JsEngineSetup> jsEngineSetup)
    {
        this.loggerFactory = loggerFactory;
        this.dalServices = dalServices;
        this.httpClientFactory = httpClientFactory;
        this.jsEngineSetup = jsEngineSetup;
        this.logger = loggerFactory.CreateLogger<JsInternalEngine>();
    }

    public void ExecuteModule(JsExecutionContext context)
    {
        this.logger.LogTrace("Entering to ExecuteModule for name {name}.", context.Name);
        using IDisposable? scope = this.logger.BeginScope("CronJobName: {cronJobName}", context.Name);


        using Engine engine = new Engine(options =>
        {
            options.LimitMemory(this.jsEngineSetup.Value.MemoryLimit);
            options.TimeoutInterval(this.jsEngineSetup.Value.Timeout);
            options.MaxStatements(this.jsEngineSetup.Value.MaxStatements);
            options.LimitRecursion(this.jsEngineSetup.Value.LimitRecursion);

            if (!string.IsNullOrEmpty(this.jsEngineSetup.Value.ModuleBasePath))
            {
                options.EnableModules(this.jsEngineSetup.Value.ModuleBasePath, true);
            }

            // Async calls https://github.com/sebastienros/jint/issues/1883#event-13140700291
            options.ExperimentalFeatures = ExperimentalFeature.TaskInterop;
        });

        HttpFunctions httpFunctions = new HttpFunctions(engine, this.httpClientFactory, this.jsEngineSetup.Value.Api.EnableHttpApi);
        JsLog jsLog = new JsLog(this.loggerFactory.CreateLogger<JsLog>());
        DatabaseProvider databaseProvider = new DatabaseProvider(this.dalServices);
        ProcessProvider processProvider = new ProcessProvider(this.loggerFactory.CreateLogger<ProcessProvider>(), this.jsEngineSetup.Value.Api.EnableProcesspApi);
        EnvironmentProvider environmentProvider = new EnvironmentProvider();

        engine.SetValue("__log", new Action<object?>(val =>
         {
             string value = val?.ToString() ?? "<NULL>";
             System.Diagnostics.Debug.WriteLine(value);
         }));


        engine.Modules.Add("dbApi", builder =>
        {
            builder.ExportValue("bucket", context.BucketName);
            builder.ExportObject("http", httpFunctions);
            builder.ExportObject("log", jsLog);
            builder.ExportObject("database", databaseProvider);
            builder.ExportObject("process", processProvider);
            builder.ExportObject("environment", environmentProvider);
            builder.ExportFunction("assert", (args) =>
            {
                if (!args[0].AsBoolean())
                {
                    string errorMessage = args.Length > 1 ? args[1].AsString() : "Assert failed!";
                    throw new Exception(errorMessage); //TODO
                }
            });
        });

        engine.Modules.Add("_main", context.Code);

        if (context.CheckOnly)
        {
            throw new NotImplementedException();
        }
        else
        {
            engine.Modules.Import("_main");
        }
    }
}