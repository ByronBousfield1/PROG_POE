using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Models;

namespace PROG_POE.Services;

public class ClaimService : IClaimService
{
    private readonly AppDbContext _db;
    public ClaimService(AppDbContext db) => _db = db;

    public async Task SubmitAsync(Guid claimId)
    {
        var claim = await _db.Claims.Include(c => c.Contract)
            .FirstAsync(c => c.ClaimId == claimId);

        if (claim.TotalHours <= 0)
            throw new InvalidOperationException("Cannot submit a claim with zero hours.");

        claim.Status = ClaimStatus.Submitted;
        claim.Stage = ApprovalStage.Coordinator;
        await _db.SaveChangesAsync();
    }

    public async Task ApproveAsync(Guid claimId, string byRole)
    {
        var claim = await _db.Claims.FirstAsync(c => c.ClaimId == claimId);

        if (byRole.Equals("Coordinator", StringComparison.OrdinalIgnoreCase))
        {
            claim.Status = ClaimStatus.CoordinatorApproved;
            claim.Stage = ApprovalStage.Manager;
        }
        else if (byRole.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            claim.Status = ClaimStatus.ManagerApproved;
            claim.Stage = ApprovalStage.Completed;
        }

        await _db.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid claimId, string byRole, string? reason = null)
    {
        var claim = await _db.Claims.FirstAsync(c => c.ClaimId == claimId);

        if (byRole.Equals("Coordinator", StringComparison.OrdinalIgnoreCase))
            claim.Status = ClaimStatus.CoordinatorRejected;
        else if (byRole.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            claim.Status = ClaimStatus.ManagerRejected;

        claim.Stage = ApprovalStage.Lecturer;
        await _db.SaveChangesAsync();
    }
}
