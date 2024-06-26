using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace YATsDb.Services.Implementation.JsEngine;

internal class ProcessProvider
{
    private readonly ILogger<ProcessProvider> logger;
    private readonly bool isEnabled;

    public ProcessProvider(ILogger<ProcessProvider> logger, bool isEnabled)
    {
        this.logger = logger;
        this.isEnabled = isEnabled;
    }

    public int StartProcess(IDictionary<string, object?> spParams)
    {
        this.logger.LogTrace("Entering to StartProcess.");

        this.CheckEnabled();

        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.UseShellExecute = false;
        processStartInfo.FileName = this.GetRequiredString(spParams, "path");
        processStartInfo.Arguments = this.GetOptionalString(spParams, "arguments", string.Empty);

        string workingDirectory = this.GetOptionalString(spParams, "workingDirectory", string.Empty);
        if (!string.IsNullOrEmpty(workingDirectory))
        {
            processStartInfo.WorkingDirectory = workingDirectory;
        }

        string userName = this.GetOptionalString(spParams, "userName", string.Empty);
        if (!string.IsNullOrEmpty(userName))
        {
            processStartInfo.UserName = userName;
        }

        TimeSpan timeout = TimeSpan.FromMinutes(1.0);

        string timeoutStr = this.GetOptionalString(spParams, "timeout", string.Empty);
        if (!string.IsNullOrEmpty(timeoutStr))
        {
            timeout = TimeSpan.Parse(timeoutStr);
        }

        string stdInStr = this.GetOptionalString(spParams, "stdin", string.Empty);
        if (!string.IsNullOrEmpty(stdInStr))
        {
            processStartInfo.RedirectStandardInput = true;
        }

        this.logger.LogInformation("Start process: FileName={FileName} Arguments={Arguments} WorkingDirectory={WorkingDirectory} UserName={UserName} RedirectStandardInput={RedirectStandardInput}",
            processStartInfo.FileName,
            processStartInfo.Arguments,
            processStartInfo.WorkingDirectory,
            processStartInfo.UserName,
            processStartInfo.RedirectStandardInput);

        using Process? process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new Exception();
        }

        if (!string.IsNullOrEmpty(stdInStr))
        {
            process.StandardInput.Write(stdInStr);
            process.StandardInput.Flush();
            process.StandardInput.Close();
        }

        if (!process.WaitForExit(timeout))
        {
            throw new JsApiException("Program timeout.");
        }

        this.logger.LogDebug("Process exited: {exitCode}.", process.ExitCode);

        return process.ExitCode;
    }

    private string GetRequiredString(IDictionary<string, object?> spParams, string name, [CallerMemberName] string methodName = "")
    {
        if (spParams.TryGetValue(name, out object? value))
        {
            return value?.ToString() ?? string.Empty;
        }

        throw new JsApiException($"Method {methodName} require parameter object with key {name}.");
    }

    private string GetOptionalString(IDictionary<string, object?> spParams, string name, string defaultValue, [CallerMemberName] string methodName = "")
    {
        if (spParams.TryGetValue(name, out object? value))
        {
            return value?.ToString() ?? string.Empty;
        }

        return defaultValue;
    }

    private void CheckEnabled()
    {
        if (!this.isEnabled)
        {
            throw new JsApiException("Process API is not enabled.");
        }
    }
}
