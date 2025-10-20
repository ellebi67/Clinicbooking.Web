using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models
{
    /// <summary>
    /// Tabella ponte che associa un ruolo a un tema specifico
    /// Permette di configurare quale layout usare per ogni ruolo
    /// </summary>
    public class RoleTheme
    {
        /// <summary>
        /// ID univoco dell'associazione (chiave primaria)
        /// </summary>
        public int RoleThemeId { get; set; }

        /// <summary>
        /// Nome del ruolo (deve corrispondere ai ruoli in AspNetRoles)
        /// Valori possibili: "Admin", "Secretary", "Doctor"
        /// </summary>
        [Required, StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// ID del tema associato a questo ruolo (FK verso Themes)
        /// </summary>
        [Required]
        public int ThemeId { get; set; }

        /// <summary>
        /// Proprietà di navigazione verso il tema
        /// Permette di accedere ai dettagli del tema tramite Entity Framework
        /// </summary>
        public Theme? Theme { get; set; }

        /// <summary>
        /// Indica se questo è il tema di default per il ruolo
        /// Utile se in futuro vogliamo permettere più temi per ruolo
        /// </summary>
        public bool IsDefault { get; set; } = true;
    }
}