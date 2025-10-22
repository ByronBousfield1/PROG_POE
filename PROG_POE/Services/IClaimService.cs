using PROG_POE.Models;

namespace PROG_POE.Services;

public interface IClaimService
{
    Task SubmitAsync(Guid claimId);
    Task ApproveAsync(Guid claimId, string byRole);
    Task RejectAsync(Guid claimId, string byRole, string? reason = null);
}
