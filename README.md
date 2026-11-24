# 🏨 HotelSaaS — Hotel Management Platform

A full-stack **multi-tenant Hotel Management SaaS** built with **.NET 9** (Clean Architecture) and **React 19 + Vite**. Features digital QR check-in, guest KYC, booking management, invoicing, revenue reports, and a super-admin panel.

---

## 🚀 Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | .NET 9, ASP.NET Core, Clean Architecture |
| ORM / DB | Entity Framework Core 9, MS SQL Server LocalDB |
| Auth | JWT + Refresh Tokens, PBKDF2 password hashing |
| Frontend | React 19, Vite 8, TypeScript, TailwindCSS |
| UI Components | shadcn/ui, Radix UI, Lucide Icons, Recharts |
| File Storage | Local filesystem (`wwwroot/uploads/`) |
| QR Code | QRCoder library (server-side PNG generation) |

---

## 📋 Features

- 🏨 **Multi-tenant** — multiple hotels, fully isolated per tenant
- 📱 **QR Self Check-in** — guest scans standee QR on phone, fills KYC form with Govt ID photo upload
- 🧾 **Digital Check-in Queue** — receptionist reviews, approves/rejects KYC in real-time
- 📅 **Bookings** — create, view, manage bookings with room assignment
- 🛏️ **Room Management** — room types, availability, status tracking
- 💰 **Invoicing & Billing** — GST-compliant invoices, revenue reports
- 👥 **Guest Management** — guest profiles, documents, visit history, VIP/favorites
- 📊 **Dashboard & Reports** — revenue, occupancy, GST summary
- 📣 **Campaigns** — WhatsApp/SMS marketing to guest lists
- 🔐 **Super Admin Panel** — manage all hotels and subscription plans

---

## 🛠️ Local Setup — Step by Step

### Prerequisites

Install these before cloning:

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 9.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18+ | https://nodejs.org |
| SQL Server LocalDB | (bundled with Visual Studio) | https://aka.ms/sqllocaldb |
| Git | any | https://git-scm.com |

> **Quick check:** Open a terminal and run:
> ```
> dotnet --version    # should be 9.x
> node --version      # should be 18+
> npm --version       # should be 9+
> ```

---

### Step 1 — Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/hotel-saas.git
cd hotel-saas
```

---

### Step 2 — Backend Setup

#### 2a. Navigate to the API project

```bash
cd backend/HotelSaaS.API
```

#### 2b. Restore NuGet packages

```bash
dotnet restore
```

#### 2c. Create the database and seed sample data

```bash
dotnet ef database update
```

> This runs all EF Core migrations and seeds:
> - **Grand Palace Hotel** (sample hotel)
> - Admin user: `rahul@grandpalace.com` / `Password123!`
> - Sample rooms, room types, bookings, guests

If `dotnet ef` command is not found, install the EF tools first:
```bash
dotnet tool install --global dotnet-ef
```

#### 2d. Start the backend API

```bash
dotnet run --urls "http://0.0.0.0:5208"
```

Backend will be running at **`http://localhost:5208`**

> **For mobile QR scanning on the same Wi-Fi network**, use `0.0.0.0` as the URL (already in the command above). This binds to all network interfaces so your phone can reach the API through the Vite proxy.

---

### Step 3 — Frontend Setup

Open a **new terminal** and from the project root:

#### 3a. Navigate to the frontend

```bash
cd frontend
```

#### 3b. Install npm packages

```bash
npm install
```

#### 3c. Configure environment

```bash
cp .env.example .env
```

The `.env` file should contain:
```env
VITE_API_URL=
VITE_APP_NAME=HotelSaaS
```

> ⚠️ **Important:** Keep `VITE_API_URL=` **empty**. This tells the frontend to use relative paths (`/api/...`) which Vite proxies to the backend. If you set it to `http://localhost:5208`, mobile phones on the network won't be able to make API calls.

#### 3d. Start the frontend dev server

```bash
npm run dev -- --host 0.0.0.0
```

Frontend will be running at **`http://localhost:5173`**

> `--host 0.0.0.0` is required for mobile QR scanning. Without it, only your PC can access the app.

---

### Step 4 — Allow Port 5173 Through Windows Firewall (for mobile QR scanning)

> Skip this step if you don't need mobile device access.

Open **PowerShell as Administrator** and run:

```powershell
netsh advfirewall firewall add rule name="Vite Dev Server 5173" dir=in action=allow protocol=TCP localport=5173 profile=any
```

---

### Step 5 — Open in Browser

| URL | Purpose |
|-----|---------|
| `http://localhost:5173` | Main hotel dashboard |
| `http://localhost:5173/login` | Login page |
| `http://localhost:5173/checkin/{token}` | Guest self check-in page (mobile) |

**Default login credentials (seeded):**

| Role | Email | Password |
|------|-------|----------|
| Hotel Manager | `rahul@grandpalace.com` (or `demo@grandpalace.com`) | `Password123!` |
| Super Admin | `admin@hotelsaas.com` | `Password123!` |

---

### Step 6 — Test Mobile QR Check-in (Optional)

1. Find your machine's local Wi-Fi IP:
   ```powershell
   ipconfig | findstr "IPv4"
   # e.g. 192.168.1.109
   ```

2. On your phone browser (same Wi-Fi), open:
   ```
   http://192.168.1.109:5173
   ```

3. Login as `rahul@grandpalace.com` → go to **QR Code** page

4. The QR code auto-detects your Wi-Fi IP — scan it with another phone to test guest check-in

---

## 📁 Project Structure

```
HotelSaaS/
├── backend/
│   ├── HotelSaaS.API/            # ASP.NET Core Web API (Controllers, Middleware)
│   ├── HotelSaaS.Application/    # MediatR handlers, DTOs, Validators (CQRS)
│   ├── HotelSaaS.Domain/         # Entities, Enums, Domain Exceptions, Interfaces
│   ├── HotelSaaS.Infrastructure/ # EF Core, DB Context, Migrations, Services
│   └── HotelSaaS.Tests/          # Unit tests
├── frontend/
│   ├── src/
│   │   ├── components/           # Reusable UI components (shadcn + custom)
│   │   ├── pages/                # Route-level page components
│   │   ├── services/             # API service functions (axios)
│   │   ├── hooks/                # Custom React hooks (useAuth, etc.)
│   │   ├── layouts/              # DashboardLayout, PublicLayout
│   │   └── types/                # TypeScript interfaces
│   ├── vite.config.ts            # Vite config (proxy /api → localhost:5208)
│   └── .env                      # Environment variables
├── HotelSaaS.sln                 # Visual Studio solution file
└── README.md
```

---

## ⚙️ Configuration Reference

### Backend — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=HotelSaasDb;..."
  },
  "Jwt": {
    "Secret": "CHANGE_ME_TO_A_VERY_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Storage": {
    "Provider": "Local",
    "LocalBasePath": "uploads"
  },
  "Frontend": {
    "BaseUrl": "http://localhost:5173"
  }
}
```

### Frontend — `.env`

```env
VITE_API_URL=          # Leave EMPTY — uses Vite proxy
VITE_APP_NAME=HotelSaaS
```

---

## 🔄 Vite Proxy Architecture

```
Mobile Phone
    │
    │ http://192.168.1.xxx:5173/api/...
    ▼
Vite Dev Server (port 5173)
    │
    │ Proxies /api/* → http://localhost:5208
    ▼
.NET Backend (port 5208)
    │
    ▼
SQL Server LocalDB
```

> This is why `VITE_API_URL` must be empty — relative paths let the phone's browser proxy through Vite correctly. If set to `http://localhost:5208`, the phone would try to call its own localhost (which has nothing listening).

---

## 🧪 Running Tests

```bash
dotnet test
```

---

## 🏗️ Building for Production

### Backend
```bash
cd backend/HotelSaaS.API
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd frontend
npm run build
# Output in frontend/dist/
```

---

## 🐛 Common Issues & Fixes

| Problem | Fix |
|---------|-----|
| `dotnet ef` not found | Run `dotnet tool install --global dotnet-ef` |
| Database migration error | Ensure SQL Server LocalDB is installed (comes with VS) |
| Mobile shows "Invalid Check-In" | Add Windows Firewall rule for port 5173 (see Step 4) |
| `VITE_API_URL` set to localhost | Clear it (`VITE_API_URL=`) in `frontend/.env` |
| Port 5208 already in use | Kill existing process: `Get-Process -Name "HotelSaaS.API" \| Stop-Process` |
| `dotnet ef database update` fails | Check SQL Server LocalDB: `sqllocaldb info` and `sqllocaldb start MSSQLLocalDB` |

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.
