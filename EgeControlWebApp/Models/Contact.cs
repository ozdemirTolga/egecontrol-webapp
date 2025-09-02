using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgeControlWebApp.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket seçimi zorunludur")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Ünvan/Pozisyon")]
        [StringLength(100)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Email adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Dahili")]
        [StringLength(10)]
        public string? Extension { get; set; }

        [Display(Name = "Mobil Telefon")]
        [StringLength(20)]
        public string? MobilePhone { get; set; }

        [Display(Name = "Departman")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;
        
        public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

        // Computed Property
        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [NotMapped]
        [Display(Name = "Ad Soyad (Ünvan)")]
        public string FullNameWithTitle => string.IsNullOrEmpty(Title) 
            ? FullName 
            : $"{FullName} ({Title})";
    }
}
