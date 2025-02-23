using h.Client.Services;
using h.Contracts.AuditLog;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace h.Client.Pages.Admin;

public partial class Audit
{
    private readonly IHApiClient _api;

    public Audit(IHApiClient api)
    {
        _api = api;
    }

    private async ValueTask<ItemsProviderResult<AuditLogEntryResponse>> GetRows(ItemsProviderRequest request)
    {
        var log = await _api.GetAuditLog(request.StartIndex, request.Count);

        return new ItemsProviderResult<AuditLogEntryResponse>(log.PaginatedEntries, log.TotalCount);
    }
}
