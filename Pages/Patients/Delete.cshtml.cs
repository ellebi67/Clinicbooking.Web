using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Patients;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    public DeleteModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Patient Item { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entity = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == id);
        if (entity is null) return NotFound();
        Item = entity;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var entity = await _db.Patients.FindAsync(id);
        if (entity is null) return NotFound();

        _db.Patients.Remove(entity);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
