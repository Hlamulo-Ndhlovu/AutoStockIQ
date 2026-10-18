using AutoStockIQ.Data;
using AutoStockIQ.Options;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;

namespace AutoStockIQ.Services;

public class FirestoreService
{
    private readonly FirestoreDb? _firestoreDb;
    private readonly bool _isEnabled;

    public FirestoreService(IOptions<FirebaseOptions> firebaseOptions)
    {
        var options = firebaseOptions.Value;
        _isEnabled = !string.IsNullOrEmpty(options.ProjectId) && 
                      !string.IsNullOrEmpty(options.ServiceAccountKeyPath) && 
                      File.Exists(options.ServiceAccountKeyPath);
        
        if (_isEnabled)
        {
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(options.ServiceAccountKeyPath);
            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = options.ProjectId,
                Credential = credential
            }.Build();
        }
    }

    public FirestoreDb? GetDatabase()
    {
        return _firestoreDb;
    }

    public bool IsEnabled => _isEnabled;

    // Supplier operations
    public async Task<string> CreateSupplierAsync(Supplier supplier)
    {
        if (!_isEnabled || _firestoreDb == null)
            return string.Empty;
            
        var docRef = _firestoreDb.Collection("suppliers").Document();
        supplier.Id = docRef.Id;
        await docRef.SetAsync(supplier);
        return supplier.Id;
    }

    public async Task<Supplier?> GetSupplierAsync(string id)
    {
        if (!_isEnabled || _firestoreDb == null)
            return null;
            
        var docRef = _firestoreDb.Collection("suppliers").Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Supplier>() : null;
    }

    public async Task<List<Supplier>> GetAllSuppliersAsync()
    {
        if (!_isEnabled || _firestoreDb == null)
            return new List<Supplier>();
            
        var snapshot = await _firestoreDb.Collection("suppliers").GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Supplier>()).ToList();
    }

    public async Task UpdateSupplierAsync(Supplier supplier)
    {
        if (!_isEnabled || _firestoreDb == null)
            return;
            
        var docRef = _firestoreDb.Collection("suppliers").Document(supplier.Id);
        await docRef.SetAsync(supplier, SetOptions.Overwrite);
    }

    public async Task DeactivateSupplierAsync(string id)
    {
        if (!_isEnabled || _firestoreDb == null)
            return;
            
        var docRef = _firestoreDb.Collection("suppliers").Document(id);
        var update = new Dictionary<string, object>
        {
            { "IsActive", false },
            { "DeactivatedAtUtc", DateTime.UtcNow }
        };
        await docRef.UpdateAsync(update);
    }

    // Sale operations
    public async Task<string> CreateSaleAsync(Sale sale)
    {
        if (!_isEnabled || _firestoreDb == null)
            return string.Empty;
            
        var docRef = _firestoreDb.Collection("sales").Document();
        sale.Id = docRef.Id;
        await docRef.SetAsync(sale);
        return sale.Id;
    }

    public async Task<Sale?> GetSaleAsync(string id)
    {
        if (!_isEnabled || _firestoreDb == null)
            return null;
            
        var docRef = _firestoreDb.Collection("sales").Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Sale>() : null;
    }

    public async Task<List<Sale>> GetSalesByCustomerAsync(string customerId)
    {
        if (!_isEnabled || _firestoreDb == null)
            return new List<Sale>();
            
        var snapshot = await _firestoreDb.Collection("sales")
            .WhereEqualTo("CustomerId", customerId)
            .GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Sale>()).ToList();
    }

    public async Task<List<Sale>> GetAllSalesAsync()
    {
        if (!_isEnabled || _firestoreDb == null)
            return new List<Sale>();
            
        var snapshot = await _firestoreDb.Collection("sales").GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Sale>()).ToList();
    }

    // Audit log operations
    public async Task<string> CreateAuditLogAsync(AuditLog auditLog)
    {
        if (!_isEnabled || _firestoreDb == null)
            return string.Empty;
            
        var docRef = _firestoreDb.Collection("auditLogs").Document();
        auditLog.Id = docRef.Id;
        await docRef.SetAsync(auditLog);
        return auditLog.Id;
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null, string? actionType = null)
    {
        if (!_isEnabled || _firestoreDb == null)
            return new List<AuditLog>();
            
        var query = _firestoreDb.Collection("auditLogs").OrderByDescending("TimestampUtc");

        if (startDate.HasValue)
        {
            query = query.WhereGreaterThanOrEqualTo("TimestampUtc", startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.WhereLessThanOrEqualTo("TimestampUtc", endDate.Value);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.WhereEqualTo("UserId", userId);
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            query = query.WhereEqualTo("ActionType", actionType);
        }

        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<AuditLog>()).ToList();
    }
}
