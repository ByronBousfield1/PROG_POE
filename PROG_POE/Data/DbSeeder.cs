using Microsoft.EntityFrameworkCore;
using PROG_POE.Models;

namespace PROG_POE.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Lecturers.Any()) return;

        var lect = new Lecturer { FullName = "Dr. Lerato Naidoo", StaffNo = "IC001", Department = "Computer Science" };
        var contract = new Contract { Lecturer = lect, HourlyRate = 450m, MaxMonthlyHours = 30 };
        var claim = new Claim
        {
            Contract = contract,
            Month = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
        };

        db.AddRange(lect, contract, claim);
        db.SaveChanges();
    }

    public static void Recalculate(Claim claim, AppDbContext db)
    {
        var rate = db.Contracts.Where(c => c.ContractId == claim.ContractId)
                               .Select(c => c.HourlyRate)
                               .FirstOrDefault();
        claim.TotalHours = Math.Round(claim.Lines.Sum(l => l.Hours), 2);
        claim.TotalAmount = Math.Round(claim.TotalHours * rate, 2);
        db.SaveChanges();
    }
}
