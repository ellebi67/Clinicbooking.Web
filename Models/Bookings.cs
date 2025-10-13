// ==============================
// Modello Booking (in inglese)
// UI e commenti in italiano
// ==============================
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models
{
    // La classe rappresenta un appuntamento (Booking)
    public class Booking
    {
        // Id univoco dell'appuntamento
        public int BookingId { get; set; }

        // Riferimento al paziente (FK verso tabella Patients)
        [Required] // Obbligatorio: in UI daremo un messaggio chiaro
        public int PatientId { get; set; }

        // Navigazione opzionale verso Patient (caricata quando serve)
        public Patient? Patient { get; set; }

        // Riferimento al dottore (FK verso tabella Doctors)
        [Required]
        public int DoctorId { get; set; }

        // Navigazione verso Doctor
        public Doctor? Doctor { get; set; }

        // Riferimento alla specializzazione (FK verso tabella Specializations)
        [Required]
        public int SpecializationId { get; set; }

        // Navigazione verso Specialization
        public Specialization? Specialization { get; set; }

        // Data e ora dell'appuntamento (in locale)
        [Required]
        public DateTime DateTime { get; set; }

        // Note libere (facoltative) per l'operatore
        [MaxLength(500)]
        public string? Notes { get; set; }

        // RowVersion per la concorrenza ottimistica (timestamp)
        public byte[]? RowVersion { get; set; }
    }
}
