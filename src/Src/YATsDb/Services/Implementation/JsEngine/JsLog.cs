using Jint.Native;

namespace YATsDb.Services.Implementation.JsEngine;

internal class JsLog
{
    private readonly ILogger logger;

    public JsLog(ILogger logger)
    {
        this.logger = logger;
    }

    public JsValue Trace(JsValue args)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Debug(JsValue args)
    {
        if (this.logger.IsEnabled(LogLevel.Debug))
        {
            this.logger.LogDebug("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Info(JsValue args)
    {
        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Warning(JsValue args)
    {
        if (this.logger.IsEnabled(LogLevel.Warning))
        {
            this.logger.LogWarning("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }

    public JsValue Error(JsValue args)
    {
        if (this.logger.IsEnabled(LogLevel.Error))
        {
            this.logger.LogError("JSLog: {0}", args);
        }

        return JsValue.Undefined;
    }
}
