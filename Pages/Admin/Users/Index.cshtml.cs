using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Admin.Users;

[Authorize(Roles = "Admin")] // Solo utenti con ruolo Admin possono accedere
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // Lista utenti da mostrare nella tabella
    public IList<UserViewModel> Users { get; set; } = new List<UserViewModel>();

    public async Task OnGetAsync()
    {
        // Carica tutti gli utenti con i loro ruoli
        var users = await _userManager.Users.ToListAsync();

        Users = new List<UserViewModel>();

        foreach (var user in users)
        {
            // Ottieni i ruoli per ogni utente
            var roles = await _userManager.GetRolesAsync(user);

            Users.Add(new UserViewModel
            {
                Id = user.Id,                              // ID univoco utente
                Email = user.Email ?? "",                  // Email dell'utente
                IsActive = !user.LockoutEnabled ||         // Utente attivo se non bloccato
                          user.LockoutEnd == null ||       // oppure senza scadenza blocco
                          user.LockoutEnd <= DateTimeOffset.Now, // oppure blocco scaduto
                Roles = string.Join(", ", roles)           // Lista ruoli separati da virgola
            });
        }
    }
}

// ViewModel per visualizzare dati utente in tabella
public class UserViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }
    public string Roles { get; set; } = "";
}