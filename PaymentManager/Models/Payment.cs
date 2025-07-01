using System.ComponentModel.DataAnnotations;

namespace PaymentManager.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200)]
        public string Description { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public string Category { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; } // Para evitar duplicados

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}