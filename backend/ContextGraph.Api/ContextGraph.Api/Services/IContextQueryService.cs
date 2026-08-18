using ContextGraph.Api.Models;

namespace ContextGraph.Api.Services;

public interface IContextQueryService
{
    Task<ContextQueryResponse> QueryContextAsync(
        string query,
        CancellationToken cancellationToken = default);
}