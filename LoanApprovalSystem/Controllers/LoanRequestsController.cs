
using LoanApprovalSystem.Data;
using LoanApprovalSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class LoanRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoanRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: LOANREQUESTS
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (User.IsInRole("Staff"))
        {
            return View(await _context.LoanRequests
                .Where(x => x.CreatedByUserId == userId)
                .ToListAsync());
        }

        if (User.IsInRole("Manager"))
        {
            return View(await _context.LoanRequests
                .Where(x => x.Status == "Pending Manager"
                         || x.Status == "Pending Direktur"
                         || x.Status == "Approved"
                         || x.Status.Contains("Rejected"))
                .ToListAsync());
        }

        if (User.IsInRole("Direktur"))
        {
            return View(await _context.LoanRequests
                .Where(x => x.Status == "Pending Direktur"
                         || x.Status == "Approved"
                         || x.Status.Contains("Rejected"))
                .ToListAsync());
        }

        return View(await _context.LoanRequests.ToListAsync());
    }

    // GET: LOANREQUESTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var loanrequest = await _context.LoanRequests
            .FirstOrDefaultAsync(m => m.Id == id);
        if (loanrequest == null)
        {
            return NotFound();
        }

        return View(loanrequest);
    }

    // GET: LOANREQUESTS/Create
    [Authorize(Roles = "Staff")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: LOANREQUESTS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create(LoanRequest loanRequest, IFormFile pdfFile)
    {
        // hapus validasi field yang memang akan diisi otomatis
        ModelState.Remove("LoanNumber");
        ModelState.Remove("AttachmentPath");
        ModelState.Remove("Status");
        ModelState.Remove("CurrentApproverRole");
        ModelState.Remove("CreatedByUserId");
        ModelState.Remove("CreatedAt");

        // validasi file wajib
        if (pdfFile == null || pdfFile.Length == 0)
        {
            ModelState.AddModelError("AttachmentPath", "File PDF wajib diupload.");
        }
        else
        {
            var extension = Path.GetExtension(pdfFile.FileName).ToLower();

            if (extension != ".pdf")
            {
                ModelState.AddModelError("AttachmentPath", "File harus berformat PDF.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(loanRequest);
        }

        // generate nomor peminjaman
        loanRequest.LoanNumber = "LOAN-" + DateTime.Now.ToString("yyyyMMddHHmmss");

        // status awal approval
        loanRequest.Status = "Pending Manager";
        loanRequest.CurrentApproverRole = "Manager";

        // data otomatis
        loanRequest.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        loanRequest.CreatedAt = DateTime.Now;

        // upload PDF
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);

        var uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/uploads"
        );

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await pdfFile.CopyToAsync(stream);
        }

        loanRequest.AttachmentPath = "/uploads/" + fileName;

        _context.Add(loanRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // GET: LOANREQUESTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var loanrequest = await _context.LoanRequests.FindAsync(id);
        if (loanrequest == null)
        {
            return NotFound();
        }
        return View(loanrequest);
    }

    // POST: LOANREQUESTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,LoanNumber,Title,Amount,Description,AttachmentPath,Status,CurrentApproverRole,CreatedByUserId,CreatedAt")] LoanRequest loanrequest)
    {
        if (id != loanrequest.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(loanrequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanRequestExists(loanrequest.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(loanrequest);
    }

    // GET: LOANREQUESTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var loanrequest = await _context.LoanRequests
            .FirstOrDefaultAsync(m => m.Id == id);
        if (loanrequest == null)
        {
            return NotFound();
        }

        return View(loanrequest);
    }

    // POST: LOANREQUESTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var loanrequest = await _context.LoanRequests.FindAsync(id);
        if (loanrequest != null)
        {
            _context.LoanRequests.Remove(loanrequest);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LoanRequestExists(int? id)
    {
        return _context.LoanRequests.Any(e => e.Id == id);
    }


    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ApproveManager(int id)
    {
        var loanRequest = await _context.LoanRequests.FindAsync(id);

        if (loanRequest == null)
        {
            return NotFound();
        }

        if (loanRequest.Status != "Pending Manager")
        {
            return RedirectToAction(nameof(Index));
        }

        if (loanRequest.Amount < 10000000)
        {
            loanRequest.Status = "Approved";
            loanRequest.CurrentApproverRole = "-";
        }
        else
        {
            loanRequest.Status = "Pending Direktur";
            loanRequest.CurrentApproverRole = "Direktur";
        }

        _context.Update(loanRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Direktur")]
    public async Task<IActionResult> ApproveDirektur(int id)
    {
        var loanRequest = await _context.LoanRequests.FindAsync(id);

        if (loanRequest == null)
        {
            return NotFound();
        }

        if (loanRequest.Status != "Pending Direktur")
        {
            return RedirectToAction(nameof(Index));
        }

        loanRequest.Status = "Approved";
        loanRequest.CurrentApproverRole = "-";

        _context.Update(loanRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Manager,Direktur")]
    public async Task<IActionResult> Reject(int id)
    {
        var loanRequest = await _context.LoanRequests.FindAsync(id);

        if (loanRequest == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Manager") && loanRequest.Status == "Pending Manager")
        {
            loanRequest.Status = "Rejected by Manager";
            loanRequest.CurrentApproverRole = "-";
        }
        else if (User.IsInRole("Direktur") && loanRequest.Status == "Pending Direktur")
        {
            loanRequest.Status = "Rejected by Direktur";
            loanRequest.CurrentApproverRole = "-";
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }

        _context.Update(loanRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
