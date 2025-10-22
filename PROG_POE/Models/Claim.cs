
namespace PROG_POE.Models;

public class Claim
{
    public Guid ClaimId { get; set; } = Guid.NewGuid();
    public Guid ContractId { get; set; }
    public Contract? Contract { get; set; }

    public DateOnly Month { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public ApprovalStage Stage { get; set; } = ApprovalStage.Lecturer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ClaimLine> Lines { get; set; } = new();
    public List<ClaimDocument> Documents { get; set; } = new();
}
