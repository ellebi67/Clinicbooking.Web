using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicBooking.Web.Pages.Specializations;

// Pagina creazione nuova specializzazione
public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    public CreateModel(AppDbContext db) => _db = db;

    // Modello bindato alla form
    [BindProperty] public Specialization Input { get; set; } = new();

    // GET: mostra la form vuota
    public void OnGet() { }

    // POST: salva la nuova specializzazione
    public async Task<IActionResult> OnPostAsync()
    {
        // Valida i dati (DataAnnotations su Specialization)
        if (!ModelState.IsValid) return Page();

        _db.Specializations.Add(Input);
        await _db.SaveChangesAsync();

        // Torna all'elenco
        return RedirectToPage("Index");
    }
}
