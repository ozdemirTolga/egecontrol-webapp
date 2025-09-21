using EgeControlWebApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EgeControlWebApp.Services
{
    public class PdfService : IPdfService
    {
        static PdfService()
        {
            // QuestPDF Community lisansını ayarla
            QuestPDF.Settings.License = LicenseType.Community;
            // Debug modunu kapat - production için
            QuestPDF.Settings.EnableDebugging = false;
        }

        public Task<byte[]> GenerateQuotePdfAsync(Quote quote, PdfType pdfType = PdfType.Detailed)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header().Column(headerCol =>
                    {
                        // Üst kısım: Logo ve İletişim Bilgileri
                        headerCol.Item().Row(row =>
                        {
                            // Sol: EGE AUTOMATION Logosu
                            row.RelativeItem(3).Column(logoCol =>
                            {
                                try
                                {
                                    var logoPath = Path.Combine("wwwroot", "images", "Logo.png");
                                    if (File.Exists(logoPath) && new FileInfo(logoPath).Length > 0)
                                    {
                                        var logoBytes = File.ReadAllBytes(logoPath);
                                        logoCol.Item()
                                            .MaxHeight(80)
                                            .MaxWidth(200)
                                            .Image(logoBytes);
                                    }
                                    else
                                    {
                                        logoCol.Item()
                                            .Text("EGE AUTOMATION")
                                            .Bold()
                                            .FontSize(20)
                                            .FontColor("#FF6600");
                                    }
                                }
                                catch (Exception)
                                {
                                    // Fallback to text if logo loading fails
                                    logoCol.Item()
                                        .Text("EGE AUTOMATION")
                                        .Bold()
                                        .FontSize(20)
                                        .FontColor("#FF6600");
                                }
                            });

                            // Sağ: İletişim Bilgileri
                            row.RelativeItem(2).Column(contactCol =>
                            {
                                contactCol.Item().AlignRight().Text("EGE OTOMASYON - Nurhan ÖZDEMİR").Bold().FontSize(10).FontColor("#2C5F7C");
                                contactCol.Item().AlignRight().Text("Kültür Mah. Aktuğ Sk. No:4/2 - Bolu/MERKEZ").FontSize(8);
                                contactCol.Item().AlignRight().Text("Tel: 0545 494 19 57").FontSize(8);
                            });
                        });

                        // Büyük TEKLİF FORMU başlığı
                        headerCol.Item().PaddingTop(0).Column(titleCol =>
                        {
                            titleCol.Item().AlignCenter().Text("TEKLİF FORMU")
                                .Bold().FontSize(16).FontColor("#4A90E2");
                            
                            // Mavi çizgi
                            titleCol.Item().PaddingTop(0).LineHorizontal(1).LineColor("#4A90E2");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);

                        // Üst bilgi satırı: Teklif No, Tarih, Para Birimi
                        col.Item().Row(topRow =>
                        {
                            topRow.RelativeItem().Text($"Teklif No: {quote.QuoteNumber}").Bold().FontSize(12);
                            topRow.RelativeItem().AlignCenter().Text($"Tarih: {quote.QuoteDate:dd.MM.yyyy}").FontSize(11);
                            topRow.RelativeItem().AlignRight().Text($"Para Birimi: {quote.Currency ?? "TRY"}").FontSize(11);
                        });

                        col.Item().Row(statusRow =>
                        {
                            statusRow.RelativeItem().Text($"Geçerlilik: {quote.ValidUntil:dd.MM.yyyy}").FontSize(11);
                            statusRow.RelativeItem().AlignRight().Text($"Durum: {GetStatusText(quote.Status)}").FontSize(11);
                        });

                        // Ana bilgi kutucukları
                        col.Item().PaddingTop(6).Row(infoRow =>
                        {
                            // Sol: MÜŞTERİ BİLGİLERİ (Yeşil kutu)
                            infoRow.RelativeItem().Padding(5).Border(1).BorderColor("#90EE90").Background("#F0FFF0").Column(customerCol =>
                            {
                                var cust = quote.Customer;
                                customerCol.Item().AlignCenter().Text(" MÜŞTERİ BİLGİLERİ").Bold().FontSize(12).FontColor("#006400");
                                customerCol.Item().PaddingTop(5).Text($" Şirket: {cust?.CompanyName ?? "-"}").FontSize(10);
                                customerCol.Item().Text($" Yetkili: {cust?.ContactPerson ?? "-"}").FontSize(10);
                                customerCol.Item().Text($" Email: {cust?.Email ?? "-"}").FontSize(10);
                                customerCol.Item().Text($" Telefon: {cust?.Phone ?? "-"}").FontSize(10);
                                if (!string.IsNullOrWhiteSpace(cust?.Address))
                                    customerCol.Item().Text($" Adres: {cust!.Address}").FontSize(10);
                                customerCol.Item().Text($" Şehir: {cust?.City ?? " Merkez"}").FontSize(10);
                            });

                            infoRow.ConstantItem(10); // Boşluk

                            // Sağ: TEKLİFİ OLUŞTURAN (Turuncu kutu)
                            infoRow.RelativeItem().Padding(5).Border(1).BorderColor("#FFB347").Background("#FFF8DC").Column(creatorCol =>
                            {
                                creatorCol.Item().AlignCenter().Text("TEKLİFİ OLUŞTURAN").Bold().FontSize(12).FontColor("#FF6600");
                                
                                if (quote.CreatedByUser != null)
                                {
                                    creatorCol.Item().PaddingTop(5).Text($" Ad Soyad: {quote.CreatedByUser.FirstName} {quote.CreatedByUser.LastName}").FontSize(10);
                                    creatorCol.Item().Text($" Email: {quote.CreatedByUser.Email ?? "-"}").FontSize(10);
                                    creatorCol.Item().Text($" Departman: {quote.CreatedByUser.Department ?? "IT"}").FontSize(10);
                                    creatorCol.Item().Text($" Pozisyon: {quote.CreatedByUser.Position ?? "System Administrator"}").FontSize(10);
                                    creatorCol.Item().Text($" Telefon: {quote.CreatedByUser.PhoneNumber ?? "Belirtilmemiş"}").FontSize(10);
                                    creatorCol.Item().Text($" Oluşturma: {quote.CreatedAt:dd.MM.yyyy HH:mm}").FontSize(10);
                                }
                                else
                                {
                                    creatorCol.Item().PaddingTop(5).Text($" Ad Soyad: Admin User").FontSize(10);
                                    creatorCol.Item().Text($" Email: admin@egecontrol.com").FontSize(10);
                                    creatorCol.Item().Text($" Departman: IT").FontSize(10);
                                    creatorCol.Item().Text($" Pozisyon: System Administrator").FontSize(10);
                                    creatorCol.Item().Text($" Telefon: Belirtilmemiş").FontSize(10);
                                    creatorCol.Item().Text($" Oluşturma: {quote.CreatedAt:dd.MM.yyyy HH:mm}").FontSize(10);
                                }
                            });
                        });

                        // Teklif başlığı ve açıklama
                        col.Item().PaddingTop(8).Text($"{quote.Title}").Bold().FontSize(14);
                        if (!string.IsNullOrEmpty(quote.Description))
                            col.Item().Text($"Açıklama: {quote.Description}");

                        // PDF Type'a göre farklı görünümler
                        if (pdfType == PdfType.Detailed && quote.QuoteItems.Any())
                        {
                            // DETAYLI VERSİYON - Tüm fiyat bilgileri
                            col.Item().PaddingTop(6).Text("TEKLİF KALEMLERİ (DETAYLI)").Bold().FontSize(12);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                // header
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("Ürün/Hizmet");
                                    header.Cell().Element(HeaderCell).Text("Miktar");
                                    header.Cell().Element(HeaderCell).Text("Birim");
                                    header.Cell().Element(HeaderCell).Text("Birim Fiyat");
                                    header.Cell().Element(HeaderCell).Text("İndirim");
                                    header.Cell().Element(HeaderCell).Text("Toplam");

                                    static IContainer HeaderCell(IContainer container) => container
                                        .Background("#F0F0F0")
                                        .Border(1)
                                        .BorderColor("#CCCCCC")
                                        .DefaultTextStyle(x => x.SemiBold())
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(4)
                                        .AlignCenter();
                                });

                                foreach (var item in quote.QuoteItems.OrderBy(x => x.SortOrder))
                                {
                                    // Show item name and description if available
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4)
                                        .Text(
                                            !string.IsNullOrWhiteSpace(item.ItemName)
                                                ? (!string.IsNullOrWhiteSpace(item.Description)
                                                    ? $"{item.ItemName} - {item.Description}"
                                                    : item.ItemName)
                                                : item.Description
                                        );
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text(item.Quantity.ToString("N2"));
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text(item.Unit);
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignRight().Text(CurrencyHelper.FormatCurrency(item.UnitPrice, quote.Currency ?? "TRY"));
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text($"%{item.DiscountPercentage:N1}");
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignRight().Text(CurrencyHelper.FormatCurrency(item.Total, quote.Currency ?? "TRY"));
                                }
                            });

                            // Remove VAT line and rename subtotal to total for detailed PDF version
                            //col.Item().PaddingTop(6).AlignRight().Text($"TOPLAM: {CurrencyHelper.FormatCurrency(quote.SubTotal, quote.Currency ?? "TRY")} ");
                        }
                        else if (pdfType == PdfType.Summary && quote.QuoteItems.Any())
                        {
                            // ÖZET VERSİYON - Sadece ürün isimleri, miktar ve birim
                            col.Item().PaddingTop(6).Text("TEKLİF KALEMLERİ (ÖZET)").Bold().FontSize(12);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4); // Ürün/Hizmet
                                    columns.RelativeColumn(1); // Miktar
                                    columns.RelativeColumn(1); // Birim
                                });

                                // header
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("Ürün/Hizmet");
                                    header.Cell().Element(HeaderCell).Text("Miktar");
                                    header.Cell().Element(HeaderCell).Text("Birim");

                                    static IContainer HeaderCell(IContainer container) => container
                                        .Background("#F0F0F0")
                                        .Border(1)
                                        .BorderColor("#CCCCCC")
                                        .DefaultTextStyle(x => x.SemiBold())
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(4)
                                        .AlignCenter();
                                });

                                foreach (var item in quote.QuoteItems.OrderBy(x => x.SortOrder))
                                {
                                   // table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).Text(item.ItemName);
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4)
                                        .Text(
                                            !string.IsNullOrWhiteSpace(item.ItemName)
                                                ? (!string.IsNullOrWhiteSpace(item.Description)
                                                    ? $"{item.ItemName} - {item.Description}"
                                                    : item.ItemName)
                                                : item.Description
                                        );
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text(item.Quantity.ToString("N2"));
                                    table.Cell().Border(1).BorderColor("#CCCCCC").Padding(4).AlignCenter().Text(item.Unit);
                                }
                            });

                            // Özette ara toplam ve KDV gösterme - sadece genel toplam
                        }

                        // GENEL TOPLAM - Mavi kutu içinde
                        col.Item().PaddingTop(12).AlignRight().ShrinkHorizontal().Column(totalCol =>
                        {
                            // Üst mavi başlık
                            totalCol.Item().Padding(8).Background("#4A90E2").AlignCenter()
                                .Text("GENEL TOPLAM").Bold().FontSize(12).FontColor("#FFFFFF");
                            
                            // Alt beyaz alan - Toplam tutar
                            totalCol.Item()
                                .Border(1).BorderColor("#4A90E2").Padding(10).Background("#FFFFFF").AlignCenter()
                                .Text($"TOPLAM: {CurrencyHelper.FormatCurrency(quote.SubTotal, quote.Currency ?? "TRY")} ") // KDV dahil değil, net toplam
                                .Bold().FontSize(16).FontColor("#FF0000");
                        });

                        // İmza alanı - Sadece genel toplamın altında
                        col.Item().PaddingTop(8).AlignRight().Column(signatureCol =>
                        {
                            try
                            {
                                var imagePath = Path.Combine("wwwroot", "images", "İmza.png");
                                if (File.Exists(imagePath) && new FileInfo(imagePath).Length > 0)
                                {
                                    var imageBytes = File.ReadAllBytes(imagePath);
                                    signatureCol.Item()
                                        .MaxWidth(150)
                                        .MaxHeight(100)
                                        .AlignCenter()
                                        .Image(imageBytes);
                                }
                                else
                                {
                                    signatureCol.Item()
                                        .AlignCenter()
                                        .Text("N. ÖZDEMİR")
                                        .FontSize(14)
                                        .FontColor("#1E3A8A")
                                        .Bold()
                                        .Italic();
                                }
                            }
                            catch (Exception)
                            {
                                signatureCol.Item()
                                    .AlignCenter()
                                    .Text("N. ÖZDEMİR")
                                    .FontSize(14)
                                    .FontColor("#1E3A8A")
                                    .Bold()
                                    .Italic();
                            }
                        }); // end signature column

                        // Notlar bölümü - imzadan sonra
                        if (!string.IsNullOrWhiteSpace(quote.Notes))
                        {
                            foreach (var line in quote.Notes.Split('\n'))
                                col.Item().Text(line).FontSize(10);
                        }
                    }); // end content column
                });
            });

            var bytes = document.GeneratePdf();
            return Task.FromResult(bytes);
        }

        public Task<byte[]> GenerateCustomerListPdfAsync(IEnumerable<Customer> customers)
        {
            throw new NotImplementedException();
        }

        private string GetStatusText(QuoteStatus status)
        {
            return status switch
            {
                QuoteStatus.Draft => "Taslak",
                QuoteStatus.Sent => "Gönderildi", 
                QuoteStatus.Accepted => "Kabul Edildi",
                QuoteStatus.Rejected => "Reddedildi",
                QuoteStatus.Cancelled => "İptal Edildi",
                _ => status.ToString()
            };
        }
    }
}