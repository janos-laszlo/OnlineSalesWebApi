namespace SalesWebApi.IntegrationTests;

internal static class UserUtils
{
    static int counter;
    internal static string NextEmail => $"test{counter++}@mail.com";
}
