
namespace PROG_POE.Models;

public enum ClaimStatus
{
    Draft, Submitted,
    CoordinatorApproved, CoordinatorRejected,
    ManagerApproved, ManagerRejected,
    Settled
}

public enum ApprovalStage
{
    Lecturer, Coordinator, Manager, Completed
}

