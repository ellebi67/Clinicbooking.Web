using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models;

// Entità "Specialization": rappresenta una specializzazione medica (es. Cardiologia)
public class Specialization
{
    // PK
    public int SpecializationId { get; set; }

    // Nome obbligatorio, max 60 caratteri
    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    // Navigazione inversa (opzionale): elenco dei collegamenti con i Dottori
    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
}