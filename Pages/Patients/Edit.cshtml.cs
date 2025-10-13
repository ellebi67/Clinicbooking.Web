using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Patients;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;

    public EditModel(AppDbContext db) => _db = db;

    // Modello bindato dalla form (include RowVersion)
    [BindProperty]
    public Patient Input { get; set; } = default!;

    // GET: carica il paziente da modificare
    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Trova l'entità a partire dalla chiave primaria
        var entity = await _db.Patients.FindAsync(id);

        // Se non esiste, restituisci 404
        if (entity is null) return NotFound();

        // Copia nel modello bindato per popolare la form
        Input = entity;

        // Rendi la pagina
        return Page();
    }

    // POST: tenta il salvataggio delle modifiche
    public async Task<IActionResult> OnPostAsync()
    {
        // Se i dati della form non superano la validazione, rimostra la pagina con messaggi di errore
        if (!ModelState.IsValid) return Page();

        // Recupera dal DB l'entità aggiornata (tracked)
        var entityToUpdate = await _db.Patients
                                      .FirstOrDefaultAsync(p => p.PatientId == Input.PatientId);

        // Se qualcuno l'ha cancellata nel frattempo, 404
        if (entityToUpdate is null) return NotFound();

        // Copia i campi modificabili dall'Input (proveniente dalla form) all'entità tracciata da EF
        entityToUpdate.Name = Input.Name;
        entityToUpdate.Email = Input.Email;
        entityToUpdate.Phone = Input.Phone;
        entityToUpdate.TaxCode = Input.TaxCode;

        // Imposta la RowVersion originale come "OriginalValue"
        // In questo modo EF può verificare se il record è cambiato nel frattempo
        _db.Entry(entityToUpdate).Property(p => p.RowVersion).OriginalValue = Input.RowVersion!;

        try
        {
            // Prova a salvare: se la RowVersion combacia, l'UPDATE procede
            await _db.SaveChangesAsync();

            // Tutto ok → torna all'elenco
            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Qualcun altro ha modificato il record PRIMA del nostro salvataggio:
            // la RowVersion a DB è diversa da quella postata dal form.

            // Ricarica i valori correnti dal database (senza sovrascrivere entityToUpdate)
            var dbValues = await _db.Entry(entityToUpdate).GetDatabaseValuesAsync();

            // Se è null, significa che il record è stato eliminato nel frattempo
            if (dbValues is null)
            {
                ModelState.AddModelError(string.Empty,
                    "Il paziente è stato eliminato da un altro utente.");
                return Page();
            }

            // Converte i valori correnti a un oggetto Patient
            var databasePatient = (Patient)dbValues.ToObject();

            // Messaggio chiaro per l'utente (come da tua indicazione)
            ModelState.AddModelError(string.Empty,
                "Il paziente è stato modificato da un altro utente. Ricarica i dati, verifica le differenze e riprova a salvare.");

            // Aggiorna il modello Input con i valori attuali del DB, così la pagina mostra ciò che c'è ora a sistema
            Input = databasePatient;

            // Aggiorna anche la RowVersion, altrimenti il prossimo tentativo fallirebbe comunque
            Input.RowVersion = databasePatient.RowVersion;

            // Rimostra la pagina con i nuovi valori e il messaggio
            return Page();
        }
    }
}

