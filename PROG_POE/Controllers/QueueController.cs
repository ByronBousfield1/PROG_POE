using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Models;
using PROG_POE.Services;

namespace PROG_POE.Controllers;


public class QueueController : Controller
{
    private readonly AppDbContext _db;
    private readonly IClaimService _service;
    public QueueController(AppDbContext db, IClaimService service) { _db = db; _service = service; }

    public async Task<IActionResult> Coordinator()
        => View(await _db.Claims.Include(c => c.Contract).Include(c => c.Contract!.Lecturer)
                .Where(c => c.Stage == ApprovalStage.Coordinator)
                .OrderByDescending(c => c.CreatedAt).ToListAsync());

    public async Task<IActionResult> Manager()
        => View(await _db.Claims.Include(c => c.Contract).Include(c => c.Contract!.Lecturer)
                .Where(c => c.Stage == ApprovalStage.Manager)
                .OrderByDescending(c => c.CreatedAt).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id, string role)
    {
        try { await _service.ApproveAsync(id, role); TempData["ok"] = $"{role} approved."; }
        catch (Exception ex) { TempData["err"] = ex.Message; }
        return RedirectToAction(role);
    }

    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, string role)
    {
        try { await _service.RejectAsync(id, role); TempData["ok"] = $"{role} rejected."; }
        catch (Exception ex) { TempData["err"] = ex.Message; }
        return RedirectToAction(role);
    }
}
