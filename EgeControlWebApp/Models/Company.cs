using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur")]
        [Display(Name = "Şirket Adı")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        [StringLength(500)]
        public string? Address { get; set; }

        [Display(Name = "Vergi Numarası")]
        [StringLength(20)]
        public string? TaxNumber { get; set; }

        [Display(Name = "Vergi Dairesi")]
        [StringLength(100)]
        public string? TaxOffice { get; set; }

        [Display(Name = "Website")]
        [StringLength(200)]
        public string? Website { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation Property
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }
}
