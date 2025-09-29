using EgeControlWebApp.Models;

namespace EgeControlWebApp.Services
{
    public enum PdfType
    {
        Summary,  // Özet - sadece toplam
        Detailed  // Detaylı - tüm fiyat bilgileri
    }

    public interface IPdfService
    {
        Task<byte[]> GenerateQuotePdfAsync(Quote quote, PdfType pdfType = PdfType.Detailed);
        Task<byte[]> GenerateCustomerListPdfAsync(IEnumerable<Customer> customers);
        Task<string> SaveQuotePdfAsync(Quote quote, PdfType pdfType = PdfType.Detailed);
        Task<bool> DeleteQuotePdfAsync(Quote quote);
    }
}
