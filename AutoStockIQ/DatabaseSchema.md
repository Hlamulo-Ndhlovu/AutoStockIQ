# AutoStockIQ Database Schema

```mermaid
erDiagram
    %% Users & Authentication
    AspNetUsers ||--o{ SchoolOrders : "places"
    AspNetUsers ||--o{ Sales : "processes"
    AspNetUsers ||--o{ AuditLogs : "performs"
    AspNetUsers ||--o{ AspNetUserRoles : "has"
    AspNetRoles ||--o{ AspNetUserRoles : "assigned to"
    AspNetUsers ||--o{ AspNetUserClaims : "has"
    AspNetRoles ||--o{ AspNetRoleClaims : "has"
    AspNetUsers ||--o{ AspNetUserLogins : "has"
    AspNetUsers ||--o{ AspNetUserTokens : "has"

    %% Products & Suppliers
    Products ||--o{ OrderLines : "included in"
    Products ||--o{ SaleLines : "included in"
    Products ||--o{ StockRefillAlerts : "triggers"
    Suppliers ||--o{ Products : "supplies"

    %% School Orders
    SchoolOrders ||--o{ OrderLines : "contains"

    %% Sales
    Sales ||--o{ SaleLines : "contains"

    %% User Entity
    AspNetUsers {
        string Id PK
        string SchoolName
        string BusinessName
        bool PopiaConsent
        datetime PopiaConsentDateUtc
        string PopiaConsentIpAddress
        bool IsActive
        datetime CreatedAtUtc
        string UserName
        string NormalizedUserName
        string Email
        string NormalizedEmail
        bool EmailConfirmed
        string PasswordHash
        string SecurityStamp
        string ConcurrencyStamp
        string PhoneNumber
        bool PhoneNumberConfirmed
        bool TwoFactorEnabled
        datetime LockoutEnd
        bool LockoutEnabled
        int AccessFailedCount
    }

    %% Role Entity
    AspNetRoles {
        string Id PK
        string Name
        string NormalizedName
        string ConcurrencyStamp
    }

    %% User-Role Junction
    AspNetUserRoles {
        string UserId FK
        string RoleId FK
    }

    %% Product Entity
    Products {
        int Id PK
        string Sku UK
        string Name
        string Category
        int StockQuantity
        int ReorderLevel
        decimal UnitPrice
        string SupplierId FK
        string Location
        bool IsActive
        datetime CreatedAtUtc
        datetime DeactivatedAtUtc
    }

    %% Supplier Entity
    Suppliers {
        string Id PK
        string Name
        string ContactPerson
        string Email
        string PhoneNumber
        string Address
        bool IsActive
        datetime CreatedAtUtc
        datetime DeactivatedAtUtc
    }

    %% School Order Entity
    SchoolOrders {
        int Id PK
        string OrderNumber UK
        string SchoolUserId FK
        datetime CreatedAtUtc
        int Status
        datetime SubmittedAtUtc
        datetime ApprovedAtUtc
        datetime FulfilledAtUtc
        string ApprovedByUserId
        string ApprovedByUserName
        string DecisionNote
        decimal TotalValue
    }

    %% Order Line Entity
    OrderLines {
        int Id PK
        int SchoolOrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
    }

    %% Sale Entity
    Sales {
        string Id PK
        string SaleNumber UK
        string CustomerName
        string CustomerType
        datetime CreatedAtUtc
        decimal TotalValue
        string ProcessedByUserId FK
        string ProcessedByUserName
    }

    %% Sale Line Entity
    SaleLines {
        string Id PK
        string SaleId FK
        int ProductId FK
        string ProductSku
        string ProductName
        int Quantity
        decimal UnitPrice
        decimal LineTotal
    }

    %% Stock Refill Alert Entity
    StockRefillAlerts {
        int Id PK
        int ProductId FK
        int QuantityAfter
        int ReorderLevel
        string Message
        datetime CreatedAtUtc
        string TriggeredBySchoolUserId
    }

    %% Audit Log Entity
    AuditLogs {
        string Id PK
        datetime TimestampUtc
        string UserId FK
        string UserName
        string UserEmail
        string IpAddress
        string ActionType
        string EntityType
        string EntityId
        string Description
        string Details
    }

    %% Identity Claims
    AspNetUserClaims {
        int Id PK
        string UserId FK
        string ClaimType
        string ClaimValue
    }

    AspNetRoleClaims {
        int Id PK
        string RoleId FK
        string ClaimType
        string ClaimValue
    }

    %% Identity Logins
    AspNetUserLogins {
        string LoginProvider PK
        string ProviderKey PK
        string ProviderDisplayName
        string UserId FK
    }

    %% Identity Tokens
    AspNetUserTokens {
        string UserId PK
        string LoginProvider PK
        string Name PK
        string Value
    }
```

## Legend

- **PK**: Primary Key
- **FK**: Foreign Key
- **UK**: Unique Key
- **||--o{**: One-to-Many relationship
- **||--||**: One-to-One relationship
- **}o--o{**: Many-to-Many relationship

## Key Relationships

1. **AspNetUsers ↔ SchoolOrders**: One user can place multiple school orders
2. **AspNetUsers ↔ Sales**: One user can process multiple sales
3. **SchoolOrders ↔ OrderLines**: One order contains multiple line items
4. **Sales ↔ SaleLines**: One sale contains multiple line items
5. **Products ↔ OrderLines**: One product can appear in multiple order lines
6. **Products ↔ SaleLines**: One product can appear in multiple sale lines
7. **Products ↔ StockRefillAlerts**: One product can trigger multiple refill alerts
8. **Suppliers ↔ Products**: One supplier can supply multiple products
9. **AspNetUsers ↔ AspNetRoles**: Many-to-many relationship via AspNetUserRoles

## Database File Location

The SQLite database file is located at:
```
C:\Users\OG Loner\source\repos\AutoStockIQ\AutoStockIQ\autostockiq.db
```

## Viewing the Diagram

To view this Mermaid diagram:
1. **GitHub/GitLab**: Will render automatically
2. **VS Code**: Install the "Markdown Preview Mermaid Support" extension
3. **Online**: Use https://mermaid.live/ to render the diagram
4. **Notion, Obsidian, etc.**: Many apps support Mermaid natively
