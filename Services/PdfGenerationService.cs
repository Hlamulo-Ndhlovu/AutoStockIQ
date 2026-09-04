using AutoStockIQ.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;

namespace AutoStockIQ.Services;

public class PdfGenerationService
{
    private const string BrandName = "Kwana Ka Bobedi";
    private const string BrandContact = "Contact: info@kwanakabobedi.co.za | Phone: +27 12 345 6789";

    public byte[] GeneratePurchaseOrderPdf(SchoolOrder order, ApplicationUser schoolUser, List<Product> products)
    {
        using var memoryStream = new MemoryStream();
        var pdfWriter = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(pdfWriter);
        var document = new Document(pdf);

        // Add branding header
        AddBrandingHeader(document);

        // Add order details
        document.Add(new Paragraph($"Purchase Order: {order.OrderNumber}").SetFontSize(18).SetBold());
        document.Add(new Paragraph($"Date: {order.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC"));
        document.Add(new Paragraph($"Status: {order.Status}"));
        document.Add(new Paragraph($"School: {schoolUser.SchoolName ?? schoolUser.Email}"));
        document.Add(new Paragraph($"Email: {schoolUser.Email}"));
        document.Add(new Paragraph(""));

        // Add line items table
        var table = new Table(5);
        table.AddHeaderCell("SKU");
        table.AddHeaderCell("Product Name");
        table.AddHeaderCell("Quantity");
        table.AddHeaderCell("Unit Price");
        table.AddHeaderCell("Line Total");

        foreach (var line in order.Lines)
        {
            var product = products.FirstOrDefault(p => p.Id == line.ProductId);
            table.AddCell(product?.Sku ?? line.ProductId.ToString());
            table.AddCell(product?.Name ?? "Unknown");
            table.AddCell(line.Quantity.ToString());
            table.AddCell(product?.UnitPrice.ToString("C") ?? line.UnitPrice.ToString("C"));
            table.AddCell(line.LineTotal.ToString("C"));
        }

        document.Add(table);
        document.Add(new Paragraph(""));

        // Add total
        document.Add(new Paragraph($"Total Order Value: {order.TotalValue:C}").SetFontSize(14).SetBold());

        // Add footer
        AddFooter(document, order.OrderNumber);

        document.Close();
        return memoryStream.ToArray();
    }

    public byte[] GenerateSaleReceiptPdf(Sale sale, List<SaleLine> saleLines)
    {
        using var memoryStream = new MemoryStream();
        var pdfWriter = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(pdfWriter);
        var document = new Document(pdf);

        // Add branding header
        AddBrandingHeader(document);

        // Add sale details
        document.Add(new Paragraph($"Sale Receipt: {sale.SaleNumber}").SetFontSize(18).SetBold());
        document.Add(new Paragraph($"Date: {sale.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC"));
        document.Add(new Paragraph($"Customer: {sale.CustomerName} ({sale.CustomerType})"));
        document.Add(new Paragraph($"Processed by: {sale.ProcessedByUserName}"));
        document.Add(new Paragraph(""));

        // Add line items table
        var table = new Table(5);
        table.AddHeaderCell("SKU");
        table.AddHeaderCell("Product Name");
        table.AddHeaderCell("Quantity");
        table.AddHeaderCell("Unit Price");
        table.AddHeaderCell("Line Total");

        foreach (var line in saleLines)
        {
            table.AddCell(line.ProductSku);
            table.AddCell(line.ProductName);
            table.AddCell(line.Quantity.ToString());
            table.AddCell(line.UnitPrice.ToString("C"));
            table.AddCell(line.LineTotal.ToString("C"));
        }

        document.Add(table);
        document.Add(new Paragraph(""));

        // Add total
        document.Add(new Paragraph($"Total Sale Value: {sale.TotalValue:C}").SetFontSize(14).SetBold());

        // Add footer
        AddFooter(document, sale.SaleNumber);

        document.Close();
        return memoryStream.ToArray();
    }

    public byte[] GenerateStockReportPdf(List<Product> products)
    {
        using var memoryStream = new MemoryStream();
        var pdfWriter = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(pdfWriter);
        var document = new Document(pdf);

        // Add branding header
        AddBrandingHeader(document);

        // Add report details
        document.Add(new Paragraph("Stock Report").SetFontSize(18).SetBold());
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"));
        document.Add(new Paragraph(""));

        // Add products table
        var table = new Table(7);
        table.AddHeaderCell("SKU");
        table.AddHeaderCell("Product Name");
        table.AddHeaderCell("Category");
        table.AddHeaderCell("Quantity");
        table.AddHeaderCell("Reorder Level");
        table.AddHeaderCell("Unit Price");
        table.AddHeaderCell("Location");

        foreach (var product in products.Where(p => p.IsActive))
        {
            table.AddCell(product.Sku);
            table.AddCell(product.Name);
            table.AddCell(product.Category);
            table.AddCell(product.StockQuantity.ToString());
            table.AddCell(product.ReorderLevel.ToString());
            table.AddCell(product.UnitPrice.ToString("C"));
            table.AddCell(product.Location ?? "N/A");
        }

        document.Add(table);
        document.Add(new Paragraph(""));

        // Add stock valuation
        var totalValue = products.Where(p => p.IsActive).Sum(p => p.StockQuantity * p.UnitPrice);
        document.Add(new Paragraph($"Total Stock Valuation: {totalValue:C}").SetFontSize(14).SetBold());

        // Add footer
        AddFooter(document, "Stock Report");

        document.Close();
        return memoryStream.ToArray();
    }

    public byte[] GenerateAuditReportPdf(List<AuditLog> auditLogs, DateTime? startDate, DateTime? endDate)
    {
        using var memoryStream = new MemoryStream();
        var pdfWriter = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(pdfWriter);
        var document = new Document(pdf);

        // Add branding header
        AddBrandingHeader(document);

        // Add report details
        document.Add(new Paragraph("Audit Report").SetFontSize(18).SetBold());
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"));
        
        if (startDate.HasValue && endDate.HasValue)
        {
            document.Add(new Paragraph($"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"));
        }
        
        document.Add(new Paragraph($"Total Records: {auditLogs.Count}"));
        document.Add(new Paragraph(""));

        // Add audit logs table
        var table = new Table(5);
        table.AddHeaderCell("Timestamp");
        table.AddHeaderCell("User");
        table.AddHeaderCell("Action");
        table.AddHeaderCell("Entity");
        table.AddHeaderCell("Description");

        foreach (var log in auditLogs)
        {
            table.AddCell(log.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            table.AddCell(log.UserName);
            table.AddCell(log.ActionType);
            table.AddCell($"{log.EntityType} ({log.EntityId})");
            table.AddCell(log.Description);
        }

        document.Add(table);

        // Add footer
        AddFooter(document, "Audit Report");

        document.Close();
        return memoryStream.ToArray();
    }

    private void AddBrandingHeader(Document document)
    {
        var header = new Paragraph(BrandName)
            .SetFontSize(24)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetTextAlignment(TextAlignment.CENTER);
        
        document.Add(header);
        
        var contact = new Paragraph(BrandContact)
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER);
        
        document.Add(contact);
        document.Add(new Paragraph(""));
    }

    private void AddFooter(Document document, string documentTitle)
    {
        document.Add(new Paragraph(""));
        var footer = new Paragraph($"Generated by {BrandName} - {documentTitle}")
            .SetFontSize(8)
            .SetItalic()
            .SetTextAlignment(TextAlignment.CENTER);
        document.Add(footer);
    }
}
