using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Doctors;

// Pagina conferma eliminazione dottore
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    public DeleteModel(AppDbContext db) => _db = db;

    [BindProperty] public Doctor? Item { get; set; }

    // GET: mostra i dati del dottore (senza tracking)
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entity = await _db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.DoctorId == id);
        if (entity is null) return NotFound();

        Item = entity;
        return Page();
    }

    // POST: elimina dottore (le righe ponte N–N verranno eliminate per cascata)
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var entity = await _db.Doctors.FindAsync(id);
        if (entity is null) return NotFound();

        _db.Doctors.Remove(entity);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
