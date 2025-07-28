using EmreGaleriApp.Repository.Models;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.IO;
using System;

namespace EmreGaleriApp.Service.Services
{
    public class InvoiceService : IInvoiceService
    {
        [Obsolete]
        public byte[] GenerateInvoicePdf(Order order)
        {
            // FontResolver'ı bir kere ata, burada doğrudan atıyoruz.
            // Yolu kendi projenin dizinine göre ayarla
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf");
            GlobalFontSettings.FontResolver = new FontResolver(fontPath);

            using var ms = new MemoryStream();
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Arial", 20);
            var fontRegular = new XFont("Arial", 12);

            gfx.DrawString("Fatura", fontTitle, XBrushes.Black, new XRect(0, 30, page.Width, 40), XStringFormats.Center);

            gfx.DrawString($"Sipariş No: {order.Id}", fontRegular, XBrushes.Black, new XPoint(40, 100));
            gfx.DrawString($"Kullanıcı: {order.AppUser.UserName}", fontRegular, XBrushes.Black, new XPoint(40, 120));
            gfx.DrawString($"Başlangıç Tarihi: {order.StartDate:d}", fontRegular, XBrushes.Black, new XPoint(40, 140));
            gfx.DrawString($"Bitiş Tarihi: {order.EndDate:d}", fontRegular, XBrushes.Black, new XPoint(40, 160));

            int yPoint = 190;
            foreach (var item in order.OrderItems)
            {
                gfx.DrawString($"Araç: {item.Car.Id} - Günlük Ücret: {item.Car.DailyPrice:C}", fontRegular, XBrushes.Black, new XPoint(40, yPoint));
                yPoint += 20;
            }

            gfx.DrawString($"Toplam Ücret: {order.TotalPrice:C}", fontRegular, XBrushes.Black, new XPoint(40, yPoint + 20));

            document.Save(ms);
            return ms.ToArray();
        }
    }
}
