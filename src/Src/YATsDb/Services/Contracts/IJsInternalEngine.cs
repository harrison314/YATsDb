namespace YATsDb.Services.Contracts;

public interface IJsInternalEngine
{
    void ExecuteModule(JsExecutionContext context);
}
