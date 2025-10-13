using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Pages.Doctors;

// ViewModel per la form di creazione/modifica Dottore
public class DoctorFormVM
{
    // Campi base del dottore
    public int DoctorId { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    // Elenco ID specializzazioni selezionate nella form
    public List<int> SelectedSpecializationIds { get; set; } = new();

    // Opzioni da mostrare (id, nome) - popolata nel GET
    public List<(int Id, string Name)> Options { get; set; } = new();
}
