# AutoStockIQ

Ordering and stock platform for **Kweena Ka Bobedi** clients: schools and businesses place orders; staff approve them, manage catalogue, record sales, and refill stock.

## Run in Visual Studio

1. Open `AutoStockIQ.sln` (**File → Open → Project/Solution**, not Open Folder).
2. Set startup project to **AutoStockIQ**.
3. Choose the **https** profile and press **F5**.
4. Site: `https://localhost:5001`

The app uses **SQLite** (`autostockiq.db`) so it runs without installing a database server. Tables are created automatically on first start via EF Core migrations.

## Development logins

| Portal | URL | Email | Password |
| --- | --- | --- | --- |
| Company admin | `/company/login` | `admin@autostockiq.co.za` | `ChangeMe!123` |
| Company sales | `/company/login` | `sales@autostockiq.co.za` | `ChangeMe!123` |
| School | `/school/register` then `/school/login` | (register) | (your password) |
| Business | `/business/register` then `/business/login` | (register) | (your password) |

Staff emails must end with `@autostockiq.co.za`. Seed accounts are created only in Development (`appsettings.Development.json`).

## What each role can do

- **Admin** — accept/reject school and business orders, users, reports, audit, catalogue.
- **Staff (Sales)** — catalogue, pricing, stock refills, walk-in sales, suppliers.
- **SchoolUser / BusinessUser** — browse catalogue, place orders, view order history.

## API (mobile)

- Base: `/api`
- Login: `POST /api/auth/login`
- JWT from `Jwt` settings in `appsettings.json`

## Optional services

- **Google sign-in** — set real `Authentication:Google` ClientId and ClientSecret. Placeholder values are ignored so the app still starts.
- **Firebase / Firestore** — used only when a real service-account file exists.
- **MySQL** — not installed on this machine. To move off SQLite later: install MySQL or MariaDB, add `Pomelo.EntityFrameworkCore.MySql`, point `ConnectionStrings:DefaultConnection` at the server, replace migrations, and run `dotnet ef database update`.
