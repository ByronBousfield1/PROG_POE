using System.Security.Claims;
namespace PROG_POE.Models;



public class Contract
{
    public Guid ContractId { get; set; } = Guid.NewGuid();
    public Guid LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal HourlyRate { get; set; }
    public int MaxMonthlyHours { get; set; } = 30;

    public List<Claim> Claims { get; set; } = new();
}

