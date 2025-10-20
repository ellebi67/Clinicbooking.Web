using Microsoft.AspNetCore.Authorization;            // Per [Authorize]
using Microsoft.AspNetCore.Identity;                 // Per UserManager
using Microsoft.AspNetCore.Mvc;                      // Per IActionResult
using Microsoft.AspNetCore.Mvc.RazorPages;           // Per PageModel
using System;                                        // Per DateTimeOffset
using System.Threading.Tasks;                        // Per async/await

namespace ClinicBooking.Web.Pages.Admin.Users;

// Solo gli Admin possono disabilitare utenti
[Authorize(Roles = "Admin")]
public class DisableModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public DisableModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager; // Salviamo il servizio per usarlo nei metodi
    }

    // Id dell'utente da disabilitare (binding automatico da route)
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    // Email mostrata a video per conferma
    public string Email { get; set; } = string.Empty;

    // GET: recupera l'utente e mostra un semplice riepilogo
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.FindByIdAsync(Id);
        if (user is null) return NotFound();

        Email = user.Email ?? user.UserName ?? "(sconosciuta)";
        return Page();
    }

    // POST: imposta LockoutEnd a una data molto lontana nel futuro per "disabilitare" l'utente
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.FindByIdAsync(Id);
        if (user is null) return NotFound();

        // Imposta un lockout di lungo periodo (es. 100 anni)
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            // In caso di errore torniamo alla pagina per mostrare i messaggi
            var u = await _userManager.FindByIdAsync(Id);
            Email = u?.Email ?? "(sconosciuta)";
            return Page();
        }

        // Torna alla lista utenti
        return RedirectToPage("Index");
    }
}
