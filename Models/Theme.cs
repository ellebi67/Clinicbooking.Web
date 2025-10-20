using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models
{
    /// <summary>
    /// Rappresenta un tema/layout disponibile nel sistema
    /// Ogni tema ha un nome, una descrizione e un file CSS associato
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// ID univoco del tema (chiave primaria)
        /// </summary>
        public int ThemeId { get; set; }

        /// <summary>
        /// Nome del tema (es: "Admin Standard", "Secretary Professional")
        /// </summary>
        [Required, StringLength(100)]
        public string ThemeName { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione del tema per l'admin
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Nome del file di layout Razor associato (es: "_LayoutAdmin.cshtml")
        /// </summary>
        [Required, StringLength(255)]
        public string LayoutFileName { get; set; } = string.Empty;

        /// <summary>
        /// Nome del file CSS associato (es: "admin-theme.css")
        /// </summary>
        [Required, StringLength(255)]
        public string CssFileName { get; set; } = string.Empty;

        /// <summary>
        /// Indica se il tema è attivo e utilizzabile
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data di creazione del tema
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}