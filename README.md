# Gaby's Books — Mini Library Management System

A small library app: browse books, borrow/return, search, and manage users. Built with **.NET 10**, Blazor, and supports **SQL Server** (local) or **PostgreSQL** (e.g. Neon).

---

## Features

| Area | What you get |
|------|----------------|
| **Books** | List, add, edit, delete. Genres, page count, status. Borrowed books show **return date**. |
| **Search** | By title, author, pages, genre. Reading time and ease-of-reading hints. |
| **Loans** | Check-out / check-in. **Members** see only their own borrowed books; **staff** see all. |
| **Auth** | Sign in with **Google** (or Microsoft). Roles: **Admin**, **Librarian**, **Member**. |
| **Dashboard** | **Admin & Librarian only** — totals, active loans, overdue. |
| **Manage users** | **Admin only** — change user roles (Admin / Librarian / Member). |
| **AI Agent** | Optional chat with **Gemini** (set `Gemini:ApiKey` to enable). |
| **Email** | Optional “book returned” email when SMTP is configured. |

---

## Tech stack

- **.NET 10** — Blazor Web App (Interactive Server) + Web API  
- **EF Core 10** — SQL Server or PostgreSQL (Npgsql)  
- **ASP.NET Core Identity** — Roles + Google/Microsoft OAuth  

---

## Run locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), SQL Server LocalDB (or SQL Server).

1. **Clone** and open the solution (e.g. `src/MiniLibraryManagementSystem/MiniLibraryManagementSystem.sln`).

2. **Database (first time)**  
   From `src/MiniLibraryManagementSystem`:
   ```bash
   dotnet ef database update
   ```
   If `dotnet ef` is missing: `dotnet tool install --global dotnet-ef`

3. **Config (optional)**  
   - **Development** uses **SQL Server (LocalDB)** by default; no change needed. To use PostgreSQL/Neon locally, set `DatabaseProvider` and the connection string (e.g. via [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)).  
   - For **Google sign-in**: set `Authentication:Google:ClientId` and `ClientSecret` (or User Secrets).  
   - For **email on return**: set `SmtpSettings` in config.

4. **Run**
   ```bash
   dotnet run --project src/MiniLibraryManagementSystem/MiniLibraryManagementSystem.csproj
   ```
   Or open the solution in Visual Studio and F5. Open the URL shown (e.g. `https://localhost:5001`).

---

## Deploy

See **[DEPLOYMENT.md](DEPLOYMENT.md)** for production setup: Neon (PostgreSQL), Render/Azure, env vars, and Google OAuth.

---

## API (short)

- `GET/POST/PUT/DELETE /api/books` — Books CRUD  
- `GET /api/genres` — Genres  
- `GET /api/search?q=...&author=...&genreId=...` — Search  
- `GET /api/loans` — Loans (`?activeOnly=true` for current)  
- `POST /api/loans/check-out`, `POST /api/loans/{id}/check-in` — Borrow / return  
- `GET /api/users`, `PUT /api/users/{id}/roles` — User/roles (Admin only)  
- `POST /api/agent/chat` — AI chat (when Gemini is configured)  

Swagger at `/swagger` when running.
