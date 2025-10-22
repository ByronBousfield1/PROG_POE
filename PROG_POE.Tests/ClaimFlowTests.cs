using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PROG_POE.Data;
using PROG_POE.Models;
using PROG_POE.Services;
using Xunit;

namespace PROG_POE.Tests;

public class ClaimFlowTests
{
    // ---- helpers -----------------------------------------------------------

    private static AppDbContext NewDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // new isolated DB each test
            .Options;

        var db = new AppDbContext(opts);
        DbSeeder.Seed(db);
        return db;
    }

    private static void AddHours(AppDbContext db, Claim claim, decimal hours)
    {
        db.ClaimLines.Add(new ClaimLine
        {
            ClaimId = claim.ClaimId,
            WorkDate = DateOnly.FromDateTime(DateTime.Today),
            Hours = hours,
            Note = "Teaching"
        });
        db.SaveChanges();
        DbSeeder.Recalculate(claim, db);
    }

    private static IFormFile FakeFile(string name, int bytes)
    {
        var ms = new MemoryStream(new byte[bytes]);
        return new FormFile(ms, 0, ms.Length, "file", name);
    }

    // ---- tests -------------------------------------------------------------

    [Fact]
    public async Task Submit_MovesToCoordinator_AndSetsSubmittedStatus()
    {
        using var db = NewDb();
        var claim = db.Claims.First();
        AddHours(db, claim, 2);

        var svc = new ClaimService(db);
        await svc.SubmitAsync(claim.ClaimId);

        var updated = db.Claims.First();
        Assert.Equal(ClaimStatus.Submitted, updated.Status);
        Assert.Equal(ApprovalStage.Coordinator, updated.Stage);
    }

    [Fact]
    public async Task Submit_Fails_WhenNoHours()
    {
        using var db = NewDb();
        var claim = db.Claims.First();
        var svc = new ClaimService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.SubmitAsync(claim.ClaimId));

        Assert.Contains("zero hours", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ClaimStatus.Draft, claim.Status);
        Assert.Equal(ApprovalStage.Lecturer, claim.Stage);
    }

    [Fact]
    public async Task Approvals_MoveClaimToCompleted_WithManagerApprovedStatus()
    {
        using var db = NewDb();
        var claim = db.Claims.First();
        AddHours(db, claim, 3);

        var svc = new ClaimService(db);
        await svc.SubmitAsync(claim.ClaimId);
        await svc.ApproveAsync(claim.ClaimId, "Coordinator");
        await svc.ApproveAsync(claim.ClaimId, "Manager");

        var final = db.Claims.First();
        Assert.Equal(ApprovalStage.Completed, final.Stage);
        Assert.Equal(ClaimStatus.ManagerApproved, final.Status);
    }

    [Fact]
    public void Recalculate_ComputesTotals_FromLinesAndHourlyRate()
    {
        using var db = NewDb();
        var claim = db.Claims.Include(c => c.Contract).First();

        // 1.5h + 2h = 3.5h
        db.ClaimLines.AddRange(
            new ClaimLine { ClaimId = claim.ClaimId, WorkDate = DateOnly.FromDateTime(DateTime.Today), Hours = 1.5m },
            new ClaimLine { ClaimId = claim.ClaimId, WorkDate = DateOnly.FromDateTime(DateTime.Today), Hours = 2m }
        );
        db.SaveChanges();
        DbSeeder.Recalculate(claim, db);

        var expected = Math.Round(3.5m * claim.Contract!.HourlyRate, 2);
        Assert.Equal(3.5m, claim.TotalHours);
        Assert.Equal(expected, claim.TotalAmount);
    }

    [Fact]
    public async Task UploadValidation_AllowsPdfDocxXlsx_AndRejectsOthers()
    {
        // this exercises LocalFileStorage validation logic (extension + size)
        var env = new FakeEnv(); // minimal IWebHostEnvironment
        var storage = new LocalFileStorage(env);

        // allowed files
        await AssertUploadOk(storage, "supporting.pdf");
        await AssertUploadOk(storage, "evidence.docx");
        await AssertUploadOk(storage, "sheet.xlsx");

        // wrong type
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            storage.SaveAsync(FakeFile("image.jpg", 1000), "test"));

        // too large (over 5MB)
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            storage.SaveAsync(FakeFile("big.pdf", 6 * 1024 * 1024), "test"));
    }

    private static async Task AssertUploadOk(IFileStorage storage, string filename)
    {
        var path = await storage.SaveAsync(FakeFile(filename, 1000), "test");
        Assert.Contains("uploads", path);
    }

    // ---- tiny fake env for LocalFileStorage --------------------------------
    private sealed class FakeEnv : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public FakeEnv()
        {
            WebRootPath = Path.GetFullPath("wwwroot");
            Directory.CreateDirectory(WebRootPath); // ensure it exists for tests
            WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(WebRootPath);

            ContentRootPath = Directory.GetCurrentDirectory();
            ContentRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(ContentRootPath);

            ApplicationName = "Tests";
            EnvironmentName = "Development";
        }

        public string ApplicationName { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
        public string ContentRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
        public string EnvironmentName { get; set; }
    }
}