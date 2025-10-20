using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Services
{
    /// <summary>
    /// Servizio che gestisce la selezione del layout in base al ruolo utente
    /// Consulta il database per trovare il tema associato al ruolo
    /// </summary>
    public class LayoutService : ILayoutService
    {
        // Dependency Injection del database context
        private readonly AppDbContext _context;

        // Logger per tracciare operazioni e debug
        private readonly ILogger<LayoutService> _logger;

        /// <summary>
        /// Costruttore con dependency injection
        /// </summary>
        public LayoutService(AppDbContext context, ILogger<LayoutService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene il layout corretto per l'utente in base ai suoi ruoli
        /// </summary>
        /// <param name="userRoles">Lista ruoli utente (es: ["Admin"], ["Doctor"], ecc.)</param>
        /// <returns>Nome file layout (es: "_LayoutAdmin.cshtml")</returns>
        public async Task<string> GetLayoutForUserAsync(IList<string> userRoles)
        {
            try
            {
                // 1. Se l'utente non ha ruoli, usa il layout di default
                if (userRoles == null || !userRoles.Any())
                {
                    _logger.LogWarning("Utente senza ruoli, uso layout di default");
                    return "_Layout"; // Layout di default
                }

                // 2. Prendi il PRIMO ruolo dell'utente
                // Nota: un utente dovrebbe avere un solo ruolo nel nostro sistema
                // Ma per sicurezza prendiamo il primo se ne ha più di uno
                string primaryRole = userRoles.First();

                // 3. Cerca nel database il tema associato a questo ruolo
                var roleTheme = await _context.RoleThemes
                    .Include(rt => rt.Theme) // Carica anche i dati del tema (JOIN)
                    .Where(rt => rt.RoleName == primaryRole
                              && rt.IsDefault
                              && rt.Theme!.IsActive)
                    .Select(rt => rt.Theme!.LayoutFileName) // Prendi solo il nome del layout
                    .FirstOrDefaultAsync();

                // 4. Se trovato, ritorna il layout associato
                if (!string.IsNullOrEmpty(roleTheme))
                {
                    _logger.LogInformation(
                        "Layout trovato per ruolo {Role}: {Layout}",
                        primaryRole,
                        roleTheme
                    );
                    return roleTheme;
                }

                // 5. Fallback: se non trovato nel DB, usa layout di default
                _logger.LogWarning(
                    "Nessun layout configurato per ruolo {Role}, uso default",
                    primaryRole
                );
                return "_Layout";
            }
            catch (Exception ex)
            {
                // In caso di errore, logga e usa layout di default
                _logger.LogError(
                    ex,
                    "Errore durante recupero layout per ruoli {Roles}",
                    string.Join(", ", userRoles)
                );
                return "_Layout"; // Fallback sicuro
            }
        }

        /// <summary>
        /// Ottiene il file CSS corretto per l'utente in base ai suoi ruoli
        /// </summary>
        /// <param name="userRoles">Lista ruoli utente</param>
        /// <returns>Nome file CSS (es: "admin-theme.css")</returns>
        public async Task<string> GetCssForUserAsync(IList<string> userRoles)
        {
            try
            {
                // Logica identica a GetLayoutForUserAsync ma per il CSS

                if (userRoles == null || !userRoles.Any())
                {
                    return "site.css"; // CSS di default
                }

                string primaryRole = userRoles.First();

                var roleTheme = await _context.RoleThemes
                    .Include(rt => rt.Theme)
                    .Where(rt => rt.RoleName == primaryRole
                              && rt.IsDefault
                              && rt.Theme!.IsActive)
                    .Select(rt => rt.Theme!.CssFileName)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(roleTheme))
                {
                    _logger.LogInformation(
                        "CSS trovato per ruolo {Role}: {Css}",
                        primaryRole,
                        roleTheme
                    );
                    return roleTheme;
                }

                _logger.LogWarning(
                    "Nessun CSS configurato per ruolo {Role}, uso default",
                    primaryRole
                );
                return "site.css";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Errore durante recupero CSS per ruoli {Roles}",
                    string.Join(", ", userRoles)
                );
                return "site.css"; // Fallback sicuro
            }
        }
    }
}