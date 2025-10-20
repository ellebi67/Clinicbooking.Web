namespace ClinicBooking.Web.Services
{
    /// <summary>
    /// Interface per il servizio di gestione layout dinamici
    /// Definisce i metodi per ottenere il tema/layout corretto per un utente
    /// </summary>
    public interface ILayoutService
    {
        /// <summary>
        /// Ottiene il nome del file di layout da usare per l'utente corrente
        /// </summary>
        /// <param name="userRoles">Lista dei ruoli dell'utente (può avere più ruoli)</param>
        /// <returns>Nome del file layout (es: "_LayoutAdmin.cshtml")</returns>
        Task<string> GetLayoutForUserAsync(IList<string> userRoles);

        /// <summary>
        /// Ottiene il nome del file CSS da usare per l'utente corrente
        /// </summary>
        /// <param name="userRoles">Lista dei ruoli dell'utente</param>
        /// <returns>Nome del file CSS (es: "admin-theme.css")</returns>
        Task<string> GetCssForUserAsync(IList<string> userRoles);
    }
}