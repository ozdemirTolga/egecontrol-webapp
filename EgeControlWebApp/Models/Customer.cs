using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur")]
        [Display(Name = "Şirket Adı")]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "İletişim kişisi zorunludur")]
        [Display(Name = "İletişim Kişisi")]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Şehir")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Ülke")]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Display(Name = "Website")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? Website { get; set; }

        [Display(Name = "Vergi Numarası")]
        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [Display(Name = "Vergi Dairesi")]
        [StringLength(100)]
        public string? TaxOffice { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }
}
