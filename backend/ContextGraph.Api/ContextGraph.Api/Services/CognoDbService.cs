using Neo4j.Driver;

namespace ContextGraph.Api.Services;

public class CognoDbService
{
    private readonly IDriver _driver;

    public CognoDbService()
    {
        var uri = Environment.GetEnvironmentVariable("COGNODB_URI");
        var username = Environment.GetEnvironmentVariable("COGNODB_USERNAME");
        var password = Environment.GetEnvironmentVariable("COGNODB_PASSWORD");

        if (string.IsNullOrWhiteSpace(uri))
            throw new InvalidOperationException("COGNODB_URI is not configured.");

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("COGNODB_USERNAME is not configured.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("COGNODB_PASSWORD is not configured.");

        _driver = GraphDatabase.Driver(
            uri,
            AuthTokens.Basic(username, password));
    }

    public async Task<bool> TestConnectionAsync()
    {
        await _driver.VerifyConnectivityAsync();
        return true;
    }

    public async Task<string> RunFirstQueryAsync()
    {
        await using var session = _driver.AsyncSession();

        var result = await session.RunAsync(
            "RETURN 'Hello CognoDB' AS message");

        var record = await result.SingleAsync();

        return record["message"].As<string>();
    }
}