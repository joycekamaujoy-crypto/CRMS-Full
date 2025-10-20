# 🚗 Car Rental Management System (CRMS): Real-Time GPS Fleet Tracking

This repository contains the complete source code for a modern, secure, full-stack **Car Rental Management System (CRMS)** designed for peer-to-peer (P2P) vehicle sharing.

The primary "wow factor" is the **Real-Time GPS Tracking** feature, which utilizes a simulated background service and SignalR to provide instant vehicle location updates on a Leaflet map.

---

## ✨ Key Features

This application is built on a clean, layered architecture, ensuring scalability and maintainability.

| Feature Area | Description | Primary Technology |
| :--- | :--- | :--- |
| **Real-Time Tracking** | Live visualization of vehicle movement (simulated Nairobi $\leftrightarrow$ Thika route) using SignalR and Leaflet.js. | SignalR, Leaflet.js |
| **Secure Authentication** | Role-based access control (Owner/Renter) secured by **JWT (JSON Web Tokens)**. | ASP.NET Identity Hashing |
| **Vehicle Management (CRUD)** | Owners can securely add, update, and manage their fleet inventory. | ASP.NET Core Web API, EF Core |
| **Booking & Workflow** | Renters can request bookings. Owners must approve/reject requests, completing the rental lifecycle. Includes a simulated **M-Pesa payment gateway**. | Clean Service Layer, Status Enums |
| **Data Persistence** | Optimized SQL Server schema with performance indexing, particularly for high-volume telemetry data. | EF Core, SQL Server |

---

## 🏗️ Architecture Overview

The system is structured into a backend API and an MVC frontend, strictly separated for maintainability.

### 1. Backend: CarRental.Api (Web API)

The API is the central data provider, built on the principles of **Clean Architecture** with dedicated layers for Domain (Entities), Application (Services/Logic), and Infrastructure (Data/DB Context).

| Component | Responsibility | Key Dependencies |
| :--- | :--- | :--- |
| **AuthService** | User registration, login, password hashing, and JWT generation. | `IPasswordHasher<User>` |
| **BookingService** | Handling rental logic, availability checks, and status updates (Owner-only approval). | EF Core, Business Validation |
| **TelemetryService** | Persists GPS points and immediately broadcasts updates to the **SignalR Hub**. | `IHubContext<TelemetryHub>` |
| **GpsSimulatorService** | Background service (`IHostedService`) that generates and pushes realistic, timed telemetry data for the Nairobi $\leftrightarrow$ Thika route. | `BackgroundService` |

### 2. Frontend: CarRental.Frontend (ASP.NET Core MVC)

The UI layer communicates exclusively with the backend API via the centralized `ApiService`.

| Component | Responsibility | Key Dependencies |
| :--- | :--- | :--- |
| **ApiService** | Centralized `HttpClient` handler. Manages adding the **JWT** to all outbound requests. | `HttpClient`, Session Storage |
| **Controllers (MVC)** | Handle routing, model binding, session management, and role-based redirect/access control. | `[Authorize]`, Session, `IApiService` |
| **Views (.cshtml)** | Present the UI using **Tailwind CSS**, including the critical **Leaflet.js** and **SignalR** client scripts for the map view. | Tailwind CSS, Leaflet.js, SignalR |

---

## 🛠️ Technical Stack

| Category | Technology | Version | Notes |
| :--- | :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core Web API | .NET 8 | Secure, cross-platform RESTful API. |
| **Database** | SQL Server Developer Ed. | N/A | Persistent, indexed data storage. |
| **ORM** | Entity Framework Core | .NET 8 | Fluent API for schema configuration. |
| **Real-Time** | ASP.NET Core SignalR | .NET 8 | WebSocket-based data transmission. |
| **Frontend Framework** | ASP.NET Core MVC | .NET 8 | Server-side rendering for the web application. |
| **Styling** | Tailwind CSS | Latest CDN/CLI | Utility-first, responsive design. |
| **Mapping** | Leaflet.js | CDN | Lightweight, open-source mapping library. |
| **Auth** | JWT Bearer Authentication | N/A | Stateless, secure API authentication. |

---

## 🚀 Getting Started

### Prerequisites

You must have the following installed locally:
1.  **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
2.  **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** (Developer Edition preferred)
3.  **[Node.js and npm](https://nodejs.org/en/download)** (Required for the Tailwind CSS CLI)

### 1. Backend Setup

This configures the database, security, and real-time simulator services.

1.  **Configure Connection String:** Update the `ConnectionStrings` section in `CarRental.Api/appsettings.Development.json` with your SQL Server details (ensure it supports mixed authentication).
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=CarRentalDB;User Id=YourUser;Password=YourPassword;TrustServerCertificate=True;"
    }
    ```
2.  **Apply Migrations:** Navigate to the `CarRental.Api` project directory in your terminal and run the following commands to create the database schema:
    ```bash
    dotnet ef database update
    ```

### 2. Frontend Setup (Tailwind CLI)

The frontend uses the Tailwind CLI for an optimized build.

1.  **Install Dependencies:** Navigate to the `CarRental.Frontend` project directory:
    ```bash
    cd CarRental.Frontend
    npm install
    ```
2.  **Start Tailwind Watcher:** Open a **separate terminal window** and run the watch script. This must be running constantly to compile your CSS changes.
    ```bash
    npm run watch
    ```

### 3. Run the Application

You will need **two running terminals**: one for the API (backend) and one for the frontend.

1.  **Terminal 1 (Backend API):**
    ```bash
    cd CarRental.Api
    dotnet run
    ```
2.  **Terminal 2 (Frontend MVC):**
    ```bash
    cd CarRental.Frontend
    dotnet run
    ```

The application should now be accessible at `https://localhost:7001` (or a similar port).

---

## 💡 Usage & Demonstration

### Test Accounts

Since this is a PoC, you can register new users via the API's Register endpoint (or mock data in the service layer).

| Role | Email Example | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Owner** | owner@example.com | 123456 | Full access to Car CRUD, Rental Approval, Tracking. |
| **Renter** | renter@example.com | 123456 | Can view available cars, create bookings, view history. |

### Real-Time GPS Tracking Demo

1.  Log in as the **Owner**.
2.  Navigate to the **Tracking** page.
3.  The Leaflet map will initialize, centered near Nairobi.
4.  Wait for the `GpsSimulatorService` to start sending data (it begins automatically on API startup).
5.  You will see vehicle markers appear and begin moving smoothly along the defined route: **Nairobi CBD $\rightarrow$ Thika $\rightarrow$ Nairobi CBD**.
6.  The **Live Telemetry Console** below the map will show the raw data arriving via SignalR every 5 seconds.

---

## 🐳 Deployment (Containerization Goal)

The final step for production readiness is containerization. The architecture is designed for ease of deployment, utilizing environment variables for connection strings.

Future steps will include providing a `Dockerfile` and a `docker-compose.yml` file to containerize the `CarRental.Api` and `CarRental.Frontend` services, enabling scalable, cloud-native deployment.

---
