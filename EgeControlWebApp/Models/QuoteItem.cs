using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EgeControlWebApp.Models
{
    public class QuoteItem
    {
        public int Id { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "Ürün/Hizmet adı zorunludur")]
        [Display(Name = "Ürün/Hizmet")]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Miktar zorunludur")]
        [Display(Name = "Miktar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır")]
        public decimal Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Birim fiyat zorunludur")]
        [Display(Name = "Birim Fiyat")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Birim")]
        [StringLength(20)]
        public string Unit { get; set; } = "Adet";

        [Display(Name = "İndirim Oranı (%)")]
        [Range(0, 100, ErrorMessage = "İndirim oranı 0-100 arasında olmalıdır")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "İndirim Tutarı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Toplam Tutar")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Display(Name = "Sıra")]
        public int SortOrder { get; set; }

    // Navigation property
    [ForeignKey("QuoteId")]
    [ValidateNever]
    [BindNever]
    public virtual Quote? Quote { get; set; }
    }
}
