using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models;

// Entità "Doctor": rappresenta un medico
public class Doctor
{
    // PK
    public int DoctorId { get; set; }

    // Nome e cognome del medico - obbligatorio
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    // Email (facoltativa) con convalida formale
    [EmailAddress]
    public string? Email { get; set; }

    // Telefono (facoltativo)
    [Phone]
    public string? Phone { get; set; }

    // Navigazione N–N verso le specializzazioni tramite tabella ponte
    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    public ICollection<PatientDoctor> PatientDoctors { get; set; } = new List<PatientDoctor>();
}