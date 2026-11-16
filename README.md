# AutoStockIQ — Kwena ka Bobedi Inventory Management & Stock Control Ecosystem

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)
![Hosting](https://img.shields.io/badge/Hosting-Render-blueviolet)
![Platform](https://img.shields.io/badge/Platform-ASP.NET%20Core%20MVC%20%7C%20Web-blue)
![Team](https://img.shields.io/badge/Team-ProKreatives-orange)

## 📖 Project Overview

**AutoStockIQ** is a unified inventory management and procurement ecosystem built for **Kwena ka Bobedi (Pty) Ltd**. It bridges the communication gap between educational institutions (schools) and stationery/LTSM suppliers by providing a **single source of truth** across our web-based platform surfaces:

1. **Web Application (ASP.NET Core MVC):** Secure portals for school administrators, business clients, and company staff to browse catalogues, place orders, approve requests, and manage stock.
2. **Public Marketing Website:** Static HTML/CSS/JS informational portal for client onboarding, pricing tiers, POPIA compliance details, and quote requests.

---

## 🏗️ System Architecture & Tech Stack

* **Backend & Web Portal:** ASP.NET Core MVC (C#), Entity Framework Core.
* **Hosting & Deployment:** Hosted live on **Render**.
* **Database:** Relational DBMS with normalized tables (`Users`, `Products`, `SchoolOrders`, `OrderLines`, `Suppliers`, `AuditLogs`) populated with $\ge 10$ records per table.
* **Security:** Encrypted/hashed password storage, Role-Based Access Control (`Admin`, `School User`, `Business User`, `Staff`), Single Sign-On (SSO), and POPIA consent logging.
* **DevOps & CI/CD:** GitHub Version Control and GitHub Actions automated build/test pipelines.

---

## 🖥️ System Screenshots & Walkthrough

### 1. Staff Portal Entry & Authentication (Role Selection & Sign-In)
*Role-separated portal entry ensuring staff and school accounts maintain dedicated sign-in experiences, allowing company users to select between Administrator and Sales roles before authenticating securely.*

#### Staff Portal Landing Page
<img width="234" height="303" alt="staff_landing_page" src="https://github.com/user-attachments/assets/65875e49-877d-4440-be40-d15dc4e64624" />


#### Role Selection (Administrator / Sales)
<img width="440" height="406" alt="staff login" src="https://github.com/user-attachments/assets/327c8d67-eb1b-4dd5-ad81-285c995450c5" />


#### Staff Sign-In Form
<img width="439" height="383" alt="staff_Login_admin" src="https://github.com/user-attachments/assets/097e3bd4-8383-4503-8cc7-9eedc4956dc4" />


---

### 2. School & Business Client Portal (Catalogue & Ordering)
*Real-time stock visibility, line-item order placement, and automated order tracking (`ORD-XXXX`).*

### School landing Page
<img width="436" height="507" alt="image" src="https://github.com/user-attachments/assets/c3419454-1660-446d-bdd3-d39bd396ede5" />
<img width="381" height="358" alt="image" src="https://github.com/user-attachments/assets/b082584b-770d-432b-ae3e-c9bb96b00f4c" />

### Registration and Business Sign-in
<img width="431" height="487" alt="image" src="https://github.com/user-attachments/assets/bfa9af67-6591-45e6-acbc-ec82e4d6a5bb" />




#### Live Stock Catalogue
<img width="1171" height="635" alt="Screenshot 2026-11-10 205207" src="https://github.com/user-attachments/assets/b748509a-60be-4db5-84c5-c2f2de34a619" />



#### Place Order Flow
<img width="376" height="495" alt="Orders" src="https://github.com/user-attachments/assets/8c66bef6-2603-427a-a2af-ffef75999719" />


#### Order History & Status
<img width="371" height="515" alt="Screenshot 2026-11-15 102400" src="https://github.com/user-attachments/assets/09346012-31ff-432a-ae9c-eac8dce08055" />


---

### 3. Company Admin & Sales Operations
*Order approval/rejection workflows, low-stock alerts, and stock replenishment.*

#### Admin Dashboard
<img width="445" height="509" alt="Ad_dasboard" src="https://github.com/user-attachments/assets/f17a0ec1-10db-479d-8e78-a8f946abe080" />


#### Pending Order Approval
<img width="433" height="414" alt="image" src="https://github.com/user-attachments/assets/af7c88e2-6e6d-4e29-a186-aa7bd86fc68c" />

#### Stock Refill & Alerts
<img width="455" height="507" alt="image" src="https://github.com/user-attachments/assets/db968156-4b28-4802-90c3-1abfff7fc1b8" />

---

### 4. Public Website & Web Portal
*Stakeholder-facing marketing website and browser-based MVC interface.*

#### Kwena ka Bobedi Public Site
<img width="1351" height="597" alt="Screenshot 2026-11-15 092300" src="https://github.com/user-attachments/assets/3a905644-e4d5-46cd-a0f0-97d7f8bbcc4e" />


#### ASP.NET Core MVC Web Portal
![ASP.NET Core MVC Web Portal](assets/screenshots/mvc_portal.png)

---

## 🧪 Quality Assurance & Testing Evidence

### Automated Testing & CI/CD (GitHub Actions)
Every push and pull request triggers our automated GitHub Actions pipeline to compile the solution and execute unit/integration test suites.

![GitHub Actions Pipeline](assets/screenshots/github_actions_pass.png)

### Database Verification ($\ge 10$ Records per Table)
All relational database tables are seeded and verified to ensure full transactional integrity and encrypted credential storage.

![Database Records Proof](assets/screenshots/database_tables.png)

### System & Negative Testing Summary

| Test ID | Feature / Screen | Test Scenario | Expected Outcome | Status |
| :--- | :--- | :--- | :--- | :--- |
| **TC-01** | Authentication | Login with invalid email / blank password | Friendly inline error message; no crash | ✅ Pass |
| **TC-02** | Security | Inspect `Users` table in DBMS | Passwords are securely hashed/encrypted | ✅ Pass |
| **TC-03** | Order Placement | Submit negative or non-numeric stock quantity | Input blocked with validation alert | ✅ Pass |
| **TC-04** | Stock Control | Admin approves school order | Order status updates; stock quantity deducts | ✅ Pass |
| **TC-05** | Web Deployment | Access live system hosted on Render | Connects successfully; all views responsive | ✅ Pass |

---

## 🚀 Setup & Installation Instructions

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

### Local Development
1. Clone the repository:
   ```bash
   git clone https://github.com/Hlamulo-Ndhlovu/AutoStockIQ.git
   cd AutoStockIQ
   ```
2. Open `AutoStockIQ.sln` in Visual Studio, or run:
   ```bash
   dotnet run --project AutoStockIQ/AutoStockIQ.csproj
   ```
3. SQLite database `autostockiq.db` is created automatically on first start.

### Deploy on Render (Docker)

1. In [Render Dashboard](https://dashboard.render.com) → **New** → **Web Service**.
2. Connect the GitHub repo `Hlamulo-Ndhlovu/AutoStockIQ`.
3. Settings:
   - **Runtime:** Docker
   - **Dockerfile path:** `./Dockerfile` (repo root)
   - **Branch:** `main`
   - **Instance type:** Free (or Starter)
4. Environment:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
5. Click **Create Web Service** and wait for the build/deploy.
6. Open the `*.onrender.com` URL. Staff logins:
   - Admin: `admin@autostockiq.co.za` / `ChangeMe!123`
   - Sales: `sales@autostockiq.co.za` / `ChangeMe!123`

**Database note:** The app uses **SQLite**. On Render’s free plan the filesystem is ephemeral, so the database resets when the service redeploys or sleeps. To keep data, attach a **Render Disk** at `/var/data` and set `DATA_DIRECTORY=/var/data`, or move to Render PostgreSQL later.

You can also deploy from `render.yaml` via **New** → **Blueprint**.

---

## 📚 User Support & Documentation
* **In-App Help:** Accessible via the **Help / Support** option inside the User Settings menu.
* **User Manual:** Full step-by-step guide located in [`docs/User_Manual.md`](docs/User_Manual.md).

---

## 👥 Team ProKreatives

| Name | Student Number | Role |
| :--- | :--- | :--- |
| **Hlamulo Ndhlovu** | ST10354799 | Lead Full-Stack Developer |
| **Simphiwe Dladla** | ST10270158 | Web Developer / Frontend & Project Manager |
| **Mbuso Sihlali** | ST10346881 | Quality Assurance (QA) & System Integrator |
| **Mudodzwa Matshakatini** | ST10440341 | UI/UX Designer & Documentation |
