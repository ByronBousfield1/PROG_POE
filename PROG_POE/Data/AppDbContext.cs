using Microsoft.EntityFrameworkCore;
using PROG_POE.Models;

namespace PROG_POE.Data;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimLine> ClaimLines => Set<ClaimLine>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
}
