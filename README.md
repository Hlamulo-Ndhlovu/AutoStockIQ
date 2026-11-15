# AutoStockIQ — Kwena ka Bobedi Inventory Management & Stock Control Ecosystem

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)
![Hosting](https://img.shields.io/badge/Hosting-Render-blueviolet)
![Platform](https://img.shields.io/badge/Platform-ASP.NET%20Core%20MVC%20%7C%20Web-blue)
![Team](https://img.shields.io/badge/Team-ProKreatives-orange)

## 📖 Project Overview
**AutoStockIQ** is a unified inventory management and procurement ecosystem built for **Kwena ka Bobedi (Pty) Ltd**. It bridges the communication gap between educational institutions (schools) and stationery/LTSM suppliers by providing a **single source of truth** across your web-based platform surfaces:

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

## 📱 System Screenshots & Walkthrough

### 1. Portal Entry & Authentication (Landing, Login, Registration & SSO)
*Role-separated portal entry ensuring staff and school accounts maintain dedicated sign-in experiences, backed by secure user registration, POPIA consent capture, encrypted password handling, and Single Sign-On.*

| Portal Entry (Staff Portal) | Login & SSO Screen | User Registration | User Settings |
| :---: | :---: | :---: | :---: |
| ![Staff Portal Entry](assets/screenshots/00_staff_portal_landing.png) | ![Login Screen](assets/screenshots/01_login.png) | ![Register Screen](assets/screenshots/02_register.png) | ![Settings Screen](assets/screenshots/03_settings.png) |

---
<img width="1351" height="597" alt="Autostock_web" src="https://github.com/user-attachments/assets/ff561529-ed73-4e82-907b-0865c1107809" />

---

### 2. School & Business Client Portal (Catalogue & Ordering)
*Real-time stock visibility, line-item order placement, and automated order tracking (`ORD-XXXX`).*

| Live Stock Catalogue | Place Order Flow | Order History & Status |
| :---: | :---: | :---: |
| ![Catalogue](assets/screenshots/04_catalogue.png) | ![Place Order](assets/screenshots/05_place_order.png) | ![Order History](assets/screenshots/06_order_history.png) |

---

### 3. Company Admin & Sales Operations
*Order approval/rejection workflows, low-stock alerts, and stock replenishment.*

| Admin Dashboard | Pending Order Approval | Stock Refill & Alerts |
| :---: | :---: | :---: |
| ![Admin Dashboard](assets/screenshots/07_admin_dashboard.png) | ![Order Approval](assets/screenshots/08_order_approval.png) | ![Stock Refill](assets/screenshots/09_stock_refill.png) |

---

### 4. Public Website & Web Portal
*Stakeholder-facing marketing website and browser-based MVC interface.*

| Kwena ka Bobedi Public Site | ASP.NET Core MVC Web Portal |
| :---: | :---: |
| ![Public Website](<img width="1351" height="597" alt="Autostock_web" src="https://github.com/user-attachments/assets/ca3a3446-31d8-4c91-ab0b-c9af6ef3f936" />
) | ![MVC Portal](assets/screenshots/11_mvc_portal.png) |

---

## 🧪 Quality Assurance & Testing Evidence

### Automated Testing & CI/CD (GitHub Actions)
Every push and pull request triggers our automated GitHub Actions pipeline to compile the solution and execute unit/integration test suites.

![GitHub Actions Pipeline](assets/screenshots/12_github_actions_pass.png)

### Database Verification ($\ge 10$ Records per Table)
All relational database tables are seeded and verified to ensure full transactional integrity and encrypted credential storage.

![Database Records Proof]<img width="807" height="400" alt="auto_dabase" src="https://github.com/user-attachments/assets/74490598-8d04-4954-97ad-5de79be62570" />


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

## 👥 Team ProKreatives
| Name | Student Number | Role |
| :--- | :--- | :--- |
| **Hlamulo Ndhlovu** | ST10354799 | Lead Full-Stack Developer |
| **Simphiwe Dladla** | ST10270158 | Web Developer / Frontend & Project Manager |
| **Mbuso Sihlali** | ST10346881 | Quality Assurance (QA) & System Integrator |
| **Mudodzwa Matshakatini** | ST10440341 | UI/UX Designer & Documentation |

