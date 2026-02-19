# Mini Library Management System

A small library management app built with **.NET 9**, Blazor, and SQL Server. It supports book management, check-in/check-out with due dates, search (title, author, pages, genre), reading time estimates, ease of reading, and **email notification when a book is returned**.

## Stack

- **.NET 9** — Blazor Web App (Interactive Server) + ASP.NET Core Web API
- **SQL Server** — LocalDB for local dev (configurable for full SQL Server)
- **Entity Framework Core 9** — ORM and migrations
- **ASP.NET Core Identity** — Auth (SSO can be added with Microsoft/Google OAuth)

## How to run locally

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB (usually installed with Visual Studio) or SQL Server

### Steps

1. **Clone and open**
   ```bash
   cd Assessment2/src
   ```

2. **Create the database (first time only)**
   ```bash
   cd MiniLibraryManagementSystem
   dotnet ef migrations add Initial
   dotnet ef database update
   cd ..
   ```
   If `dotnet ef` is not found: `dotnet tool install --global dotnet-ef`

3. **Configure (optional)**
   - Edit `MiniLibraryManagementSystem/appsettings.Development.json` to change the connection string if not using LocalDB.
   - For **email on return**, set `SmtpSettings` in `appsettings.json` (Host, Port, From, User, Password, EnableSsl).

4. **Run**
   ```bash
   dotnet run --project MiniLibraryManagementSystem/MiniLibraryManagementSystem.csproj
   ```
   Or open **MiniLibraryManagementSystem.sln** in Visual Studio and run from there.
   Open the URL shown (e.g. `https://localhost:5001`).

## Project structure

```
src/
├── MiniLibraryManagementSystem.sln
└── MiniLibraryManagementSystem/
    ├── MiniLibraryManagementSystem.csproj
    ├── Controllers/       # API: Books, Loans, Search, Genres
    ├── Data/              # ApplicationDbContext, Migrations, Seed
    ├── DTOs/              # BookDto, LoanDto, etc.
    ├── Entities/          # Book, Genre, Loan, BookStatus
    ├── Services/          # EmailNotification, ReadingTime, EaseOfReading
    └── Components/       # Blazor pages and layout
```

## Live URL

*(After deployment, add the live app URL here.)*

## API (overview)

- `GET/POST/PUT/DELETE /api/books` — Book CRUD
- `GET /api/genres` — List genres
- `GET /api/search?q=...&author=...&minPages=...&maxPages=...&genreId=...` — Search books
- `GET /api/loans` — List loans (`?activeOnly=true` for current)
- `POST /api/loans/check-out` — Body: `{ "bookId", "userId", "dueDate" }`
- `POST /api/loans/{id}/check-in` — Mark loan as returned (sends email to borrower if SmtpSettings configured)
