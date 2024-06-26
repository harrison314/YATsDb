namespace YATsDb.Services.Contracts;

public record JsExecutionContext(string BucketName, string Name, string Code, bool CheckOnly);