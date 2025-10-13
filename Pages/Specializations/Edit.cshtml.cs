using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicBooking.Web.Pages.Specializations;

// Pagina modifica specializzazione
public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    public EditModel(AppDbContext db) => _db = db;

    [BindProperty] public Specialization Input { get; set; } = default!;

    // GET: carica la specializzazione da modificare
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entity = await _db.Specializations.FindAsync(id);
        if (entity is null) return NotFound();

        Input = entity;
        return Page();
    }

    // POST: salva le modifiche
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var entity = await _db.Specializations.FindAsync(Input.SpecializationId);
        if (entity is null) return NotFound();

        // Aggiorna i campi modificabili
        entity.Name = Input.Name;

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}