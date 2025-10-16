using Microsoft.AspNetCore.Identity;

namespace ClinicBooking.Web.Data;

public static class SeedData
{
    /// <summary>
    /// Crea utenti demo per il corso se non esistono già
    /// </summary>
    public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        // Array di utenti demo da creare
        var demoUsers = new[]
        {
            new { Email = "admin@clinic.com", Password = "admin123" },      // Utente amministratore
            new { Email = "dottore@clinic.com", Password = "dott123" },     // Utente dottore  
            new { Email = "segretaria@clinic.com", Password = "segr123" }   // Utente segretaria
        };

        foreach (var userData in demoUsers)
        {
            // Controlla se l'utente esiste già
            var existingUser = await userManager.FindByEmailAsync(userData.Email);

            if (existingUser == null) // Se non esiste, crealo
            {
                var user = new IdentityUser
                {
                    UserName = userData.Email,    // Username = email
                    Email = userData.Email,       // Email dell'utente
                    EmailConfirmed = true         // Email già confermata (per semplificare corso)
                };

                // Crea l'utente con la password specificata
                var result = await userManager.CreateAsync(user, userData.Password);

                // Log del risultato per debug
                if (result.Succeeded)
                {
                    Console.WriteLine($"✅ Utente creato: {userData.Email}");
                }
                else
                {
                    Console.WriteLine($"❌ Errore creazione utente {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}