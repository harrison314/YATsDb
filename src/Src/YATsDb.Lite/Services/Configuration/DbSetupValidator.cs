
using Microsoft.Extensions.Options;

namespace YATsDb.Lite.Services.Configuration;

[OptionsValidator]
public partial class DbSetupValidator : IValidateOptions<DbSetup>
{

}