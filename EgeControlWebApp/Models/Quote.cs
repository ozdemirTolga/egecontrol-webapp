using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EgeControlWebApp.Models
{
    public class Quote
    {
        public int Id { get; set; }

        [Display(Name = "Teklif Numarası")]
        [StringLength(50)]
        public string QuoteNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Müşteri seçimi zorunludur")]
        [Display(Name = "Müşteri")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Teklif başlığı zorunludur")]
        [Display(Name = "Teklif Başlığı")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Ara Toplam")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
        public decimal SubTotal { get; set; }

        [Display(Name = "KDV Oranı (%)")]
        // [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır")] // Validation gerçek değerlerde yapılacak
        public decimal VatRate { get; set; } = 20; // Varsayılan %20 KDV

        [Display(Name = "KDV Tutarı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }

        [Display(Name = "Genel Toplam")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Teklif Tarihi")]
        public DateTime QuoteDate { get; set; } = DateTime.Now;

        [Display(Name = "Geçerlilik Tarihi")]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "Durum")]
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        [Display(Name = "Para Birimi")]
        [StringLength(3)]
        public string Currency { get; set; } = "EUR";

        [Display(Name = "Notlar")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Oluşturan")]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Oluşturan Kullanıcı ID")]
        public string? CreatedByUserId { get; set; }

        [Display(Name = "Son Düzenleyen")]
        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        [Display(Name = "Son Düzenleyen Kullanıcı ID")]
        public string? LastModifiedByUserId { get; set; }

    // Navigation properties
    [ForeignKey("CustomerId")]
    [ValidateNever]
    [BindNever]
    public virtual Customer? Customer { get; set; }

    [ValidateNever]
    [BindNever]
    public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    
    // User tracking navigation properties
    [ForeignKey("CreatedByUserId")]
    [ValidateNever]
    [BindNever]
    public virtual ApplicationUser? CreatedByUser { get; set; }
    
    [ForeignKey("LastModifiedByUserId")]
    [ValidateNever]
    [BindNever]
    public virtual ApplicationUser? LastModifiedByUser { get; set; }
        
        // For Razor Pages indexing support
        [NotMapped]
        public List<QuoteItem> QuoteItemsList 
        { 
            get => QuoteItems.ToList(); 
            set => QuoteItems = value; 
        }
    }

    public enum QuoteStatus
    {
        [Display(Name = "Taslak")]
        Draft = 0,
        [Display(Name = "Gönderildi")]
        Sent = 1,
        [Display(Name = "Kabul Edildi")]
        Accepted = 2,
        [Display(Name = "Reddedildi")]
        Rejected = 3,
        [Display(Name = "İptal Edildi")]
        Cancelled = 4
    }

    public enum CurrencyType
    {
        [Display(Name = "Euro (€)")]
        EUR,
        [Display(Name = "Türk Lirası (₺)")]
        TRY,
        [Display(Name = "Amerikan Doları ($)")]
        USD
    }
}
