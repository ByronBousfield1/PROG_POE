using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Models;
using PROG_POE.Services;

namespace PROG_POE.Controllers;


public class ClaimsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IFileStorage _files;
    private readonly IClaimService _service;

    public ClaimsController(AppDbContext db, IFileStorage files, IClaimService service)
    {
        _db = db; _files = files; _service = service;
    }

    private async Task<Contract> CurrentContractAsync()
        => await _db.Contracts.Include(c => c.Lecturer).FirstAsync();

    public async Task<IActionResult> Index()
    {
        var claims = await _db.Claims.Include(c => c.Contract)
                        .OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(claims);
    }

    public async Task<IActionResult> Create()
    {
        var contract = await CurrentContractAsync();
        var claim = new Claim
        {
            ContractId = contract.ContractId,
            Contract = contract,
            Month = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
        };
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Edit), new { id = claim.ClaimId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var claim = await _db.Claims
            .Include(c => c.Lines)
            .Include(c => c.Documents)
            .Include(c => c.Contract)
            .FirstAsync(c => c.ClaimId == id);
        return View(claim);
    }

    [HttpPost]
    public async Task<IActionResult> AddLine(Guid id, DateOnly workDate, decimal hours, string? note)
    {
        try
        {
            var claim = await _db.Claims
                .Include(c => c.Lines)
                .Include(c => c.Contract)
                .FirstAsync(c => c.ClaimId == id);
            claim.Lines.Add(new ClaimLine { ClaimId = id, WorkDate = workDate, Hours = hours, Note = note });
            DbSeeder.Recalculate(claim, _db);
            TempData["ok"] = "Line added.";
        }
        catch (Exception ex) { TempData["err"] = $"Error adding line: {ex.Message}"; }
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveLine(Guid id, Guid lineId)
    {
        try
        {
            var line = await _db.ClaimLines.FirstAsync(l => l.ClaimLineId == lineId && l.ClaimId == id);
            _db.ClaimLines.Remove(line);
            await _db.SaveChangesAsync();

            var claim = await _db.Claims.Include(c => c.Lines).FirstAsync(c => c.ClaimId == id);
            DbSeeder.Recalculate(claim, _db);
            TempData["ok"] = "Line removed.";
        }
        catch (Exception ex) { TempData["err"] = $"Error removing line: {ex.Message}"; }
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Submit(Guid id)
    {
        try { await _service.SubmitAsync(id); TempData["ok"] = "Submitted to Coordinator."; }
        catch (Exception ex) { TempData["err"] = $"Submit failed: {ex.Message}"; }
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Upload(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) { TempData["err"] = "No file selected."; return RedirectToAction(nameof(Edit), new { id }); }
        try
        {
            var relative = await _files.SaveAsync(file, id.ToString());
            _db.ClaimDocuments.Add(new ClaimDocument { ClaimId = id, FileName = file.FileName, StoredPath = relative });
            await _db.SaveChangesAsync();
            TempData["ok"] = "File uploaded.";
        }
        catch (Exception ex) { TempData["err"] = $"Upload failed: {ex.Message}"; }
        return RedirectToAction(nameof(Edit), new { id });
    }
}
