
namespace PROG_POE.Models;

public class ClaimLine
{
    public Guid ClaimLineId { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public Claim? Claim { get; set; }

    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string? Note { get; set; }
}
