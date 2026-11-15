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

* **Staff Portal Landing Page**
  <img width="234" height="303" alt="staff_landing_page" src="https://github.com/user-attachments/assets/74146fee-25ff-41ee-a242-d83e347314f2" />
  

* **Role Selection (Administrator / Sales)**
<img width="440" height="406" alt="staff login" src="https://github.com/user-attachments/assets/5d5ee88b-91d7-4177-820f-84c95c2715d0" />


* **Staff Sign-In Form**
<img width="439" height="383" alt="staff_Login_admin" src="https://github.com/user-attachments/assets/889f6f3d-63b7-48a1-a167-4261ed4723cf" />
---

### 2. School & Business Client Portal (Catalogue & Ordering)
*Real-time stock visibility, line-item order placement, and automated order tracking (`ORD-XXXX`).*

* **Live Stock Catalogue**
  ![Live Stock Catalogue](assets/screenshots/04_catalogue.png)

* **Place Order Flow**
  ![Place Order Flow](assets/screenshots/05_place_order.png)

* **Order History & Status**
  ![Order History & Status](assets/screenshots/06_order_history.png)

---

### 3. Company Admin & Sales Operations
*Order approval/rejection workflows, low-stock alerts, and stock replenishment.*

* **Admin Dashboard**
  ![Admin Dashboard](assets/screenshots/07_admin_dashboard.png)

* **Pending Order Approval**
  ![Pending Order Approval](assets/screenshots/08_order_approval.png)

* **Stock Refill & Alerts**
  ![Stock Refill & Alerts](assets/screenshots/09_stock_refill.png)

---

### 4. Public Website & Web Portal
*Stakeholder-facing marketing website and browser-based MVC interface.*

* **Kwena ka Bobedi Public Site**
  ![Kwena ka Bobedi Public Site](assets/screenshots/10_public_website.png)

* **ASP.NET Core MVC Web Portal**
  ![ASP.NET Core MVC Web Portal](assets/screenshots/11_mvc_portal.png)

---

## 🧪 Quality Assurance & Testing Evidence

### Automated Testing & CI/CD (GitHub Actions)
Every push and pull request triggers our automated GitHub Actions pipeline to compile the solution and execute unit/integration test suites.

![GitHub Actions Pipeline](assets/screenshots/12_github_actions_pass.png)

### Database Verification ($\ge 10$ Records per Table)
All relational database tables are seeded and verified to ensure full transactional integrity and encrypted credential storage.

![Database Records Proof](assets/screenshots/13_database_tables.png)

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
* SQL Server / PostgreSQL / LocalDB instance

### Local Development
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/AutoStockIQ.git
   cd AutoStockIQ
   ```
2. Update the database connection string in `appsettings.json`.
3. Apply Entity Framework migrations and seed data:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

### Deployment on Render
1. Connect your GitHub repository to a new **Web Service** on [Render](https://render.com).
2. Set the **Environment** to `.NET Core / Docker`.
3. Configure the Build Command: `dotnet publish -c Release -o out`
4. Configure the Start Command: `dotnet out/AutoStockIQ.dll`
5. Add your production database connection string under Render **Environment Variables**.

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
