# AI Setup Prompt — HotelSaaS Local Development

Copy the text below and paste it to any AI assistant (Gemini, Claude, ChatGPT, etc.) on a new machine to get the full project running including mobile QR check-in access.

---

## PROMPT TO COPY:

```
I have cloned a HotelSaaS project from GitHub. Help me set it up completely on this Windows machine including mobile access on the same Wi-Fi network.

The project structure is:
- /backend/HotelSaaS.API  → .NET 9 ASP.NET Core API (port 5208)
- /frontend               → React 19 + Vite app (port 5173)
- Database: MS SQL Server LocalDB

Please do all of the following steps in order:

---

STEP 1 — BACKEND SETUP

1a. Navigate to backend/HotelSaaS.API and restore packages:
    dotnet restore

1b. Install EF Core tools if not already installed:
    dotnet tool install --global dotnet-ef

1c. Run database migrations to create the database schema:
    dotnet ef database update

1d. Start the backend API bound to all network interfaces (required for mobile):
    dotnet run --urls "http://0.0.0.0:5208"

Run this as a background/daemon process. Confirm it starts on port 5208.

    *** IMPORTANT — SEED DATA RUNS AUTOMATICALLY ON FIRST STARTUP ***
    When the backend starts for the first time, it auto-seeds the database with:

    HOTELS & USERS:
    - Grand Palace Hotel  (login: rahul@grandpalace.com / Password123!)
    - Royal Orchid Hotel  (login: manager@royalorchid.com / Password123!)

    ROOMS (25 rooms across 4 floors per hotel):
    - Floor 1: Rooms 101–107 (Standard Non-AC @ ₹1,499 & Deluxe AC @ ₹2,799)
    - Floor 2: Rooms 201–207 (Deluxe AC & Super Deluxe @ ₹4,199)
    - Floor 3: Rooms 301–306 (Super Deluxe & Executive Suite @ ₹6,499)
    - Floor 4: Rooms 401–405 (Executive & Presidential Suite @ ₹10,999)
    - Room statuses: mix of Available, Occupied, Cleaning, Maintenance

    GUESTS (8 realistic Indian profiles):
    - Amit Verma (Bengaluru) — 4 visits, VIP/Favorite
    - Sneha Patil (Pune) — 3 visits, VIP/Favorite
    - Vikram Malhotra (Mumbai) — 2 visits
    - Priya Iyer (Chennai) — 5 visits, VIP/Favorite
    - Rohan Gupta (Delhi) — 1 visit
    - Ananya Sen (Kolkata) — 2 visits
    - Deepak Nair (Kochi) — 1 visit
    - Meera Joshi (Ahmedabad) — 6 visits, VIP/Favorite, highest spender

    BOOKINGS:
    - 7 currently Checked-In bookings (occupying rooms)
    - 3 Confirmed bookings arriving today
    - 7 past CheckedOut bookings (last 7 days, for revenue chart)

    INVOICES & PAYMENTS:
    - 7 fully Paid invoices with GST breakdown (12% room + 5% food)
    - 1 Partially Paid invoice (₹2,000 advance, balance pending)
    - UPI and Card payment records

    DIGITAL CHECK-IN QUEUE (visible at /checkin dashboard):
    - 2 submitted QR check-in sessions waiting for receptionist approval

    RETENTION POLICIES:
    - 7-year guest document retention (Sarais Act compliance)
    - 8-year invoice retention (CGST/SGST Rules)
    - DPDP Act 2023 consent records

    If you want to RE-SEED from scratch (wipe all data):
    Drop the database and re-run migrations:
        dotnet ef database drop --force
        dotnet ef database update
    Then restart the backend — seed runs again automatically.

---

STEP 2 — FRONTEND SETUP

2a. Navigate to /frontend and install npm packages:
    npm install

2b. Create the .env file from the example:
    copy .env.example .env
    (or cp .env.example .env on Mac/Linux)

2c. CRITICAL — Make sure frontend/.env contains exactly this:
    VITE_API_URL=
    VITE_APP_NAME=HotelSaaS

    The VITE_API_URL must be EMPTY (no value after the = sign).
    DO NOT set it to http://localhost:5208.
    If it is set to localhost, mobile phones will get ERR_NETWORK
    because localhost on the phone resolves to the phone itself.

2d. Make sure vite.config.ts has these settings:
    server: {
      host: "0.0.0.0",
      port: 5173,
      allowedHosts: "all",
      proxy: {
        "/api": {
          target: "http://localhost:5208",
          changeOrigin: true,
        },
      },
    }

    If host, allowedHosts, or the proxy block are missing, add them now.

2e. Start the frontend dev server bound to all interfaces:
    npm run dev -- --host 0.0.0.0

Run this as a background/daemon process. Confirm it starts on port 5173.

---

STEP 3 — WINDOWS FIREWALL (required for mobile access)

Open PowerShell as Administrator and run:
    netsh advfirewall firewall add rule name="Vite Dev Server 5173" dir=in action=allow protocol=TCP localport=5173 profile=any

This allows phones on the same Wi-Fi to reach the Vite dev server.

---

STEP 4 — FIND THIS MACHINE'S LOCAL IP

Run:
    ipconfig | findstr "IPv4"

Note the Wi-Fi IP address (e.g. 192.168.1.xxx). NOT 127.0.0.1.

---

STEP 5 — VERIFY EVERYTHING IS WORKING

5a. Test backend:
    Invoke-WebRequest -Uri "http://localhost:5208/api/public/checkin/859FFD60-B4F1-49DF-B2EE-0FE2737CA611" -UseBasicParsing | Select StatusCode

    Should return 200.

5b. Test frontend proxy (use YOUR actual IP from Step 4):
    Invoke-WebRequest -Uri "http://YOUR_IP:5173/api/public/checkin/859FFD60-B4F1-49DF-B2EE-0FE2737CA611" -UseBasicParsing | Select StatusCode

    Should return 200.

5c. Test frontend page:
    Invoke-WebRequest -Uri "http://YOUR_IP:5173" -UseBasicParsing | Select StatusCode

    Should return 200.

---

STEP 6 — ACCESS FROM BROWSER AND PHONE

Dashboard (desktop browser):
    http://localhost:5173

Login credentials (seeded):
    Email:    rahul@grandpalace.com
    Password: Password123!

Mobile QR check-in test (phone on same Wi-Fi):
    http://YOUR_IP:5173/checkin/859ffd60-b4f1-49df-b2ee-0fe2737ca611

    This should show the guest registration form with hotel name "Grand Palace Hotel".

---

ARCHITECTURE NOTE (why VITE_API_URL must be empty):

Mobile Phone → http://YOUR_IP:5173/api/...
                        ↓
              Vite Dev Server (proxy)
                        ↓
              http://localhost:5208 (backend)
                        ↓
              SQL Server LocalDB

The Vite server proxies all /api/* requests to the backend.
This means mobile phones only need to reach port 5173 — they never
directly connect to port 5208 (which is firewalled for external connections).
Setting VITE_API_URL to anything breaks this — keep it empty.

---

TROUBLESHOOTING:

If mobile shows "Invalid Check-In Link":
→ Check VITE_API_URL is empty in frontend/.env
→ Restart the Vite server after changing .env
→ Confirm firewall rule for port 5173 was added
→ Confirm phone and PC are on the same Wi-Fi network

If "ERR_NETWORK" on mobile:
→ VITE_API_URL is set to localhost — clear it

If "ERR_CONNECTION_TIMED_OUT" on mobile:
→ Firewall rule not added yet (run Step 3)
→ Or vite.config.ts missing host: "0.0.0.0" and allowedHosts: "all"

If database migration fails:
→ Run: sqllocaldb start MSSQLLocalDB
→ Then retry: dotnet ef database update

If port 5208 is in use:
→ Run: Get-Process -Name "dotnet" | Stop-Process -Force
→ Then restart backend

If port 5173 is in use:
→ Run: netstat -ano | findstr :5173
→ Kill the PID shown: taskkill /PID <PID> /F
```

