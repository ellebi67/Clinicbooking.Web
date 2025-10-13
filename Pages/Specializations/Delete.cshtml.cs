using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Specializations;

// Pagina conferma eliminazione specializzazione
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    public DeleteModel(AppDbContext db) => _db = db;

    [BindProperty] public Specialization Item { get; set; } = default!;

    // GET: carica e mostra i dati prima della conferma
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entity = await _db.Specializations.AsNoTracking().FirstOrDefaultAsync(s => s.SpecializationId == id);
        if (entity is null) return NotFound();

        Item = entity;
        return Page();
    }

    // POST: elimina la specializzazione
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var entity = await _db.Specializations.FindAsync(id);
        if (entity is null) return NotFound();

        _db.Specializations.Remove(entity);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
