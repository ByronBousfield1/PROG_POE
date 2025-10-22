using System.Diagnostics.Contracts;
namespace PROG_POE.Models;


public class Lecturer
{
    public Guid LecturerId { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = "";
    public string StaffNo { get; set; } = "";
    public string Department { get; set; } = "";
    public List<Contract> Contracts { get; set; } = new();
}
