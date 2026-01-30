namespace SalesWebApi.IntegrationTests;

internal static class UserUtils
{
    static int counter;
    public static string NextEmail => $"test{counter++}@mail.com";
}
