namespace ClinicBooking.Web.Models;

// Tabella ponte per la relazione molti-a-molti tra Doctor e Specialization
public class DoctorSpecialization
{
    // FK verso Doctor
    public int DoctorId { get; set; }

    // FK verso Specialization
    public int SpecializationId { get; set; }

    // Proprietà di navigazione (facoltative ma utili)
    public Doctor? Doctor { get; set; }
    public Specialization? Specialization { get; set; }
}
