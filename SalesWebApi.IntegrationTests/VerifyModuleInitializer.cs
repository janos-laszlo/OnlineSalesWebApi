using System.Runtime.CompilerServices;

namespace SalesWebApi.IntegrationTests;

public static class VerifyModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() =>
        VerifierSettings.InitializePlugins();
}
