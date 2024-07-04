using Microsoft.Extensions.ObjectPool;
using System.Text;
using ZLogger;

namespace YATsDb.Lite;

internal static class LoggingExtensions
{
    private static readonly ObjectPool<StringBuilder> stringBuilderPool =
    new DefaultObjectPoolProvider().CreateStringBuilderPool();

    public static void AddZLoggerConfiguration(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("ZLogging:EnableConsole"))
        {
            loggingBuilder.AddZLoggerConsole(cfg =>
            {
                cfg.IncludeScopes = true;
                cfg.InternalErrorLogger = InternalErrorLoggerAction;
                SetupPlaintext(cfg);
            });
        }

        if (configuration.GetValue<bool>("ZLogging:EnableFile"))
        {
            string? logFilePath = configuration.GetValue<string>("ZLogging:LogFilePath");

            if (string.IsNullOrEmpty(logFilePath))
            {
                Console.Error.WriteLine("\nERROR: ZLogging:LogFilePath can not be null or empty.\n");
                throw new ApplicationException("ZLogging:LogFilePath can not be null or empty.");
            }

            loggingBuilder.AddZLoggerFile(logFilePath, cfg =>
            {
                cfg.IncludeScopes = true;
                cfg.InternalErrorLogger = InternalErrorLoggerAction;
                SetupPlaintext(cfg);
            });
        }
    }

    private static void InternalErrorLoggerAction(Exception ex)
    {
        Console.Error.WriteLine(ex.ToString());
    }

    private static void SetupPlaintext(ZLoggerOptions cfg)
    {
        cfg.UsePlainTextFormatter(formatter =>
        {
            formatter.SetPrefixFormatter($"{0} | {1} | ", (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel));
            formatter.SetSuffixFormatter($" | {0} ({1})", (in MessageTemplate template, in LogInfo info) =>
            {
                if (info.ScopeState == null || info.ScopeState.IsEmpty)
                {
                    template.Format("-", info.Category);
                }
                else
                {

                    StringBuilder sb = stringBuilderPool.Get();
                    foreach (KeyValuePair<string, object?> item in info.ScopeState.Properties)
                    {
                        sb.Append(item.Key);
                        sb.Append('=');
                        sb.Append(item.Value);
                        sb.Append(' ');
                    }
                    template.Format(sb, info.Category);
                    stringBuilderPool.Return(sb);
                }
            });
            // formatter.SetExceptionFormatter((writer, ex) => Utf8String.Format(writer, $"{ex.Message}"));
        });
    }
}
