using System.ComponentModel.DataAnnotations;                 // Per validazione
using Microsoft.AspNetCore.Authorization;                    // Per [Authorize]
using Microsoft.AspNetCore.Identity;                         // Per UserManager/RoleManager
using Microsoft.AspNetCore.Mvc;                              // Per IActionResult
using Microsoft.AspNetCore.Mvc.RazorPages;                   // Per PageModel
using Microsoft.AspNetCore.Mvc.Rendering;                    // Per SelectListItem
using System.Linq;                                           // Per LINQ
using System.Threading.Tasks;                                // Per async/await

namespace ClinicBooking.Web.Pages.Admin.Users;

// Solo gli amministratori possono modificare utenti
[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EditModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;     // Servizio per operazioni sugli utenti
        _roleManager = roleManager;     // Servizio per operazioni sui ruoli
    }

    // Id dell'utente da modificare - arriva dalla route {id}
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    // Email mostrata (non modificabile)
    public string Email { get; set; } = string.Empty;

    // Tutti i ruoli disponibili (per il select multiple)
    public List<SelectListItem> AllRoles { get; set; } = new();

    // Ruoli selezionati attualmente per l'utente
    [BindProperty]
    [Display(Name = "Ruoli")]
    public List<string> SelectedRoles { get; set; } = new();

    // Campi per reimpostare la password (opzionali)
    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Nuova password")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La password deve avere almeno {2} caratteri.")]
    public string? NewPassword { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Conferma password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Le password non coincidono")]
    public string? ConfirmPassword { get; set; }

    // GET: carica utente e popolazione ruoli
    public async Task<IActionResult> OnGetAsync()
    {
        // Carica utente per Id
        var user = await _userManager.FindByIdAsync(Id);
        if (user is null) return NotFound();

        // Mostra email
        Email = user.Email ?? user.UserName ?? "(sconosciuta)";

        // Carica tutti i ruoli disponibili nel sistema
        AllRoles = _roleManager.Roles
                               .OrderBy(r => r.Name)
                               .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                               .ToList();

        // Carica ruoli già assegnati all'utente
        var userRoles = await _userManager.GetRolesAsync(user);
        SelectedRoles = userRoles.ToList();

        return Page();
    }

    // POST: salva ruoli e, se richiesto, reimposta password
    public async Task<IActionResult> OnPostAsync()
    {
        // Ricostruisce la lista ruoli (necessario per ridisegnare la pagina in caso di errori)
        AllRoles = _roleManager.Roles
                               .OrderBy(r => r.Name)
                               .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                               .ToList();

        var user = await _userManager.FindByIdAsync(Id);
        if (user is null) return NotFound();

        // Aggiorna assegnazioni ruoli: calcoliamo differenze
        var currentRoles = await _userManager.GetRolesAsync(user);
        var toAdd = SelectedRoles.Except(currentRoles).ToArray();
        var toRemove = currentRoles.Except(SelectedRoles).ToArray();

        if (toAdd.Any())
        {
            var addRes = await _userManager.AddToRolesAsync(user, toAdd);
            if (!addRes.Succeeded)
            {
                foreach (var e in addRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                // Ricarichiamo email per la pagina
                Email = user.Email ?? user.UserName ?? "(sconosciuta)";
                return Page();
            }
        }

        if (toRemove.Any())
        {
            var remRes = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!remRes.Succeeded)
            {
                foreach (var e in remRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                Email = user.Email ?? user.UserName ?? "(sconosciuta)";
                return Page();
            }
        }

        // Se è stata richiesta la reimpostazione della password, usiamo il token standard di Identity
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);      // Genera token sicuro
            var resetRes = await _userManager.ResetPasswordAsync(user, token, NewPassword); // Applica nuova password
            if (!resetRes.Succeeded)
            {
                foreach (var e in resetRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                Email = user.Email ?? user.UserName ?? "(sconosciuta)";
                return Page();
            }
        }

        // Tutto ok → torna alla lista
        return RedirectToPage("Index");
    }
}
