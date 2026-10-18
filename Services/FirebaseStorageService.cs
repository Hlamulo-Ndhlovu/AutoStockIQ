using AutoStockIQ.Options;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace AutoStockIQ.Services;

public class FirebaseStorageService
{
    private readonly StorageClient? _storageClient;
    private readonly string _bucketName;
    private readonly bool _isEnabled;

    public FirebaseStorageService(IOptions<FirebaseOptions> firebaseOptions)
    {
        var options = firebaseOptions.Value;
        _isEnabled = !string.IsNullOrEmpty(options.ProjectId) && 
                      !string.IsNullOrEmpty(options.ServiceAccountKeyPath) && 
                      File.Exists(options.ServiceAccountKeyPath) &&
                      !string.IsNullOrEmpty(options.StorageBucket);
        
        if (_isEnabled)
        {
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(options.ServiceAccountKeyPath);
            _storageClient = StorageClient.Create(credential);
            _bucketName = options.StorageBucket;
        }
        else
        {
            _bucketName = string.Empty;
        }
    }

    public async Task<string> UploadPdfAsync(string fileName, byte[] pdfData, string folder = "pdfs")
    {
        if (!_isEnabled || _storageClient == null)
            return string.Empty;
            
        var objectName = $"{folder}/{Guid.NewGuid()}/{fileName}";
        
        using var memoryStream = new MemoryStream(pdfData);
        await _storageClient.UploadObjectAsync(_bucketName, objectName, "application/pdf", memoryStream);
        
        return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
    }

    public async Task<byte[]> DownloadPdfAsync(string objectName)
    {
        if (!_isEnabled || _storageClient == null)
            return Array.Empty<byte>();
            
        using var memoryStream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(_bucketName, objectName, memoryStream);
        return memoryStream.ToArray();
    }

    public async Task DeletePdfAsync(string objectName)
    {
        if (!_isEnabled || _storageClient == null)
            return;
            
        await _storageClient.DeleteObjectAsync(_bucketName, objectName);
    }

    public async Task<string> UploadPurchaseOrderPdfAsync(string orderNumber, byte[] pdfData)
    {
        if (!_isEnabled)
            return string.Empty;
            
        var fileName = $"purchase-order-{orderNumber}.pdf";
        return await UploadPdfAsync(fileName, pdfData, "purchase-orders");
    }

    public async Task<string> UploadSaleReceiptPdfAsync(string saleNumber, byte[] pdfData)
    {
        if (!_isEnabled)
            return string.Empty;
            
        var fileName = $"sale-receipt-{saleNumber}.pdf";
        return await UploadPdfAsync(fileName, pdfData, "sale-receipts");
    }

    public async Task<string> UploadStockReportPdfAsync(byte[] pdfData)
    {
        if (!_isEnabled)
            return string.Empty;
            
        var fileName = $"stock-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return await UploadPdfAsync(fileName, pdfData, "stock-reports");
    }

    public async Task<string> UploadAuditReportPdfAsync(byte[] pdfData, DateTime? startDate, DateTime? endDate)
    {
        if (!_isEnabled)
            return string.Empty;
            
        var dateRange = startDate.HasValue && endDate.HasValue 
            ? $"{startDate:yyyyMMdd}-{endDate:yyyyMMdd}" 
            : DateTime.UtcNow.ToString("yyyyMMdd");
        var fileName = $"audit-report-{dateRange}.pdf";
        return await UploadPdfAsync(fileName, pdfData, "audit-reports");
    }
}
