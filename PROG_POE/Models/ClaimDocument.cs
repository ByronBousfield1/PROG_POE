
namespace PROG_POE.Models;

public class ClaimDocument
{
    public Guid ClaimDocumentId { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public Claim? Claim { get; set; }
    public string FileName { get; set; } = "";
    public string StoredPath { get; set; } = "";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

