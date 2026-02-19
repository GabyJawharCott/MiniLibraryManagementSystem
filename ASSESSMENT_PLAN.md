# Mini Library Management System — Assessment Plan (.NET)

> **Purpose:** Aligned plan for building the app with **.NET** and your notes. Designed so you can run it locally and **publish it online** for the live URL.

---

## 1. Requirements Checklist (with your notes)

| Category | Requirement | Your notes / Our plan |
|----------|-------------|------------------------|
| **Core** | Book Management (add, edit, delete) | Title, author + **number of pages**, **Genre/Category** + more metadata (see §3) |
| **Core** | Check-in / Check-out | Borrowed vs returned + **returning date** (we use: Due Date + Returned Date) |
| **Core** | AI features | **Reading time estimates**, **Ease of reading**, **Notify user when book is returned** (+ optional ideas) |
| **Core** | Search | **Title, author, number of pages, Genre/Category** |
| **Deliverables** | Source code + README (how to run) | ✓ |
| **Deliverables** | Deploy app + live URL | ✓ (.NET-friendly hosting in §8) |
| **Auth** | SSO + roles & permissions | ✓ (Microsoft + Google sign-in, Admin/Librarian/Member) |
| **Bonus** | Extra creative features | Dashboard, loan history, filters, etc. (§6) |
| **Eval** | Completeness, Creativity, Quality, Usability, AI-built | ✓ |

---

## 2. Tech Stack — .NET (all C#)

You preferred **.NET** and want to **publish online**. This stack keeps everything in C# and is easy to deploy.

| Layer | Choice | Why |
|-------|--------|-----|
| **Runtime** | **.NET 9** (or .NET 10 when released) | Current LTS-style; we’ll use .NET 9 and you can retarget to 10 if needed. |
| **Backend** | **ASP.NET Core Web API** | REST API for books, loans, search, auth, AI. |
| **Frontend** | **Blazor Web App** (Interactive Server or Auto) | UI in C# (no JavaScript required), same solution, good for “intuitive” app. |
| **Database** | **SQL Server** (local) or **PostgreSQL** (optional for free cloud) | EF Core supports both; SQL Server is simple on Windows. |
| **ORM** | **Entity Framework Core 9** | Migrations, relationships, seeding (roles, sample books). |
| **Auth + SSO** | **ASP.NET Core Identity** + **Microsoft** and **Google** OAuth | Built-in; no extra cost. SSO = “Sign in with Microsoft” / “Sign in with Google.” |
| **Roles** | Identity roles: **Admin**, **Librarian**, **Member** | Stored in DB; permissions in code (e.g. only Librarian+ can check-in/out for others). |
| **AI** | **OpenAI API** or **Azure OpenAI** from C# | For reading time (optional AI), ease of reading, notifications text; we can also do reading time with a simple formula. |
| **Deploy** | **Azure App Service** or **Railway** or **Render** | All support .NET and give you a public URL. Free tiers available. |

**No JavaScript required** if we use Blazor for the UI. You’ll only need Visual Studio or VS Code + .NET SDK.

---

## 3. Data Model (with your metadata)

### Book metadata (from your notes + discussion)

| Field | Type | Your note | Extra suggestion |
|-------|------|-----------|------------------|
| Title | string | ✓ | |
| Author | string | ✓ | |
| **PageCount** | int | ✓ (number of pages) | |
| **Genre/Category** | string or FK | ✓ | Prefer **Genre** as a small table (Id, Name) so we can filter and search by it. |
| ISBN | string | — | Optional; useful for search and covers. |
| Description | string | — | Optional; good for search and AI. |
| CoverUrl | string | — | Optional; e.g. from Open Library API. |
| PublishYear | int? | — | Optional. |
| **EstimatedReadingMinutes** | int? | From AI / formula | **Reading time estimate** (see §5). |
| **EaseOfReading** | string or enum? | From AI | **Ease of reading** (e.g. Easy / Medium / Hard). |
| Status | enum | — | Available / Borrowed / Maintenance. |
| CreatedAt / UpdatedAt | DateTime | — | |

**Genre:** We can define a **Genre** table (Id, Name) and Book.GenreId. Seed a few (Fiction, Non-Fiction, Science, etc.) and use dropdown in add/edit. Alternatively, a simple string field is fine if you prefer.

### Loans (check-in / check-out + returning date)

| Field | Type | Note |
|-------|------|------|
| Id | Guid | |
| BookId | FK | |
| UserId | FK | Who borrowed. |
| **BorrowedAt** | DateTime | Check-out time. |
| **DueDate** | DateTime | **Returning date** (when they should return). |
| **ReturnedAt** | DateTime? | When actually returned; null = still borrowed. |

So: **check-out** = create Loan with ReturnedAt = null; **check-in** = set ReturnedAt. **Notify user when book is returned** = when librarian marks return, we send notification to the user who had borrowed it (§5).

### Users & roles

- Use **ASP.NET Core Identity** (IdentityUser). Add **Role** (Admin, Librarian, Member).
- Optional: **UserProfile** table for display name, avatar URL from SSO.

---

## 4. Roles & Permissions (SSO)

| Role | Can do |
|------|--------|
| **Admin** | Everything: manage books, all loans, manage users/roles. |
| **Librarian** | Add/edit/delete books, check-in/check-out any book, view all loans. |
| **Member** | View books, search, borrow/return **own** loans, see own history. |

**SSO flow:**

1. User clicks “Sign in with Microsoft” or “Sign in with Google.”
2. First time: we create an Identity user and assign default role **Member**.
3. Admin can change a user’s role to Librarian or Admin (optional extra feature).
4. All protected pages and API endpoints check role (e.g. `[Authorize(Roles = "Librarian,Admin")]`).

**Setup:** In Azure Portal / Google Cloud Console you create an OAuth app and get ClientId + ClientSecret; no extra cost for standard SSO.

---

## 5. AI Features (your list + implementation)

| Your idea | How we implement it |
|-----------|----------------------|
| **Reading time estimates** | **Option A:** Simple formula: `PageCount / 200` → hours (or × 60 → minutes). **Option B:** Optional AI that considers genre/language and returns minutes. We can store in `Book.EstimatedReadingMinutes` and show on book detail. |
| **Ease of reading** | **Option A:** Heuristic from page count + genre. **Option B:** Call OpenAI with title + short description → “Easy” / “Medium” / “Hard” (or 1–5). Store in `Book.EaseOfReading`. |
| **Notify user when book is returned** | When a Librarian/Admin sets `Loan.ReturnedAt`, backend sends a notification to the **borrower**: e.g. email (“Your book X has been returned”) or in-app notification (table `Notification`: UserId, Message, IsRead, CreatedAt). Prefer **in-app** first (simpler), add email later if you want. |

**Optional extra AI (if time):**

- **Smart search:** Natural language query → map to title/author/genre/keywords.
- **Recommendations:** “Because you borrowed X, you might like Y” (by genre/author).

We’ll **prioritise**: reading time (formula + optional AI), ease of reading (AI or heuristic), and **return notification** (in-app).

---

## 6. Search (your criteria)

- **Filters:** Title, Author, **Number of pages** (range: min–max), **Genre/Category**.
- **Search:** Free-text search on title + author (and optionally description).
- Backend: EF Core `Where` + `Contains`; optional full-text or simple filters. No extra search engine required for a mini system.

---

## 7. Extra Creative Features (to show creativity)

- **Dashboard:** Total books, currently borrowed, due soon; recent loans.
- **Due date + overdue list:** Loans with DueDate &lt; today and ReturnedAt = null; highlight overdue.
- **Loan history:** Per user and per book (who borrowed when).
- **Responsive UI:** Blazor with a bit of CSS so it works on mobile.
- **Dark/light theme:** Toggle (e.g. CSS variables + Blazor state).

We can pick 2–3 of these so the app feels complete without scope creep.

---

## 8. Project Structure (.NET solution)

```
Assessment2/
├── README.md                    # How to run + live URL
├── ASSESSMENT_PLAN.md           # This file
├── src/
│   ├── LibraryManagement.sln
│   ├── LibraryManagement.Api/           # ASP.NET Core Web API
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/
│   │   │   ├── BooksController.cs
│   │   │   ├── LoansController.cs
│   │   │   ├── SearchController.cs
│   │   │   ├── NotificationsController.cs
│   │   │   └── AccountController.cs      # or use Identity endpoints
│   │   ├── Services/
│   │   │   ├── ReadingTimeService.cs     # reading time ± AI
│   │   │   ├── EaseOfReadingService.cs   # ease of reading
│   │   │   └── NotificationService.cs    # on return
│   │   └── ...
│   ├── LibraryManagement.Web/            # Blazor Web App (frontend)
│   │   ├── Program.cs
│   │   ├── Components/
│   │   │   ├── Pages/
│   │   │   │   ├── Dashboard.razor
│   │   │   │   ├── Books/
│   │   │   │   ├── Loans/
│   │   │   │   └── Search.razor
│   │   │   └── Layout/
│   │   └── ...
│   ├── LibraryManagement.Core/            # Shared: entities, DTOs, interfaces
│   │   ├── Entities/
│   │   ├── DTOs/
│   │   └── Interfaces/
│   └── LibraryManagement.Infrastructure/ # EF Core, Identity, DB
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Migrations/
│       ├── Identity/
│       └── Seed/
├── tests/
│   └── LibraryManagement.Api.Tests/
```

**Options:**

- **Single-project:** One ASP.NET Core project that hosts both API and Blazor (Blazor Web App with API). Simpler for a small app and **one deployment**.
- **Two projects:** Api + Web (Web calls API). Clear separation; you deploy API + Web (same host or separate).

For “publish and accessible online,” we can start with **one project**: Blazor Web App that includes API endpoints (minimal APIs or a few controllers). One URL, one deploy. If you prefer strict API + Blazor client, we can split later.

---

## 9. Deployment (so it’s accessible online)

You need a **public URL**. Options that work well with .NET:

| Platform | Free tier | Notes |
|----------|-----------|--------|
| **Azure App Service** | Yes (limited) | Good .NET support; use “Publish” from Visual Studio or GitHub Actions. |
| **Railway** | Yes (trial) | Connect repo; add SQL Server/PostgreSQL; env vars; automatic deploy. |
| **Render** | Yes | Similar; supports .NET and PostgreSQL. |

**Steps (high level):**

1. Push code to GitHub.
2. Create an app on Railway (or Azure / Render).
3. Add a database (PostgreSQL or SQL Server if available).
4. Set env vars: `ConnectionStrings__DefaultConnection`, Identity keys, OAuth ClientId/ClientSecret for Microsoft/Google.
5. Deploy from main branch → you get a URL like `https://your-app.railway.app`.

README will say: “Live app: https://…”

---

## 10. Book Metadata — What Else to Add? (discussion)

You already have: **title, author, number of pages, Genre/Category.** Suggested extras:

- **ISBN** — Helps with search and fetching cover/summary from Open Library.
- **Description** — For search and for AI (ease of reading, recommendations).
- **CoverUrl** — Improves UX; can be filled from Open Library by ISBN.
- **PublishYear** — Useful for filters and display.
- **EstimatedReadingMinutes** / **EaseOfReading** — As above (AI/simple).

We can keep the first version to: Title, Author, PageCount, Genre, Description, ISBN, CoverUrl, PublishYear, and the two AI fields. You can drop CoverUrl/PublishYear if you want to keep the form smaller.

---

## 11. Decisions to Confirm (summary)

| # | Topic | Proposal | Your call |
|---|--------|----------|-----------|
| 1 | **.NET version** | .NET 9 (upgrade to 10 when you’re ready) | OK? |
| 2 | **Structure** | Single Blazor Web App + API in same project (one deploy) vs Api + Web separate | Prefer one project or two? |
| 3 | **Database** | SQL Server (local dev) + same or PostgreSQL on cloud | Any preference? |
| 4 | **Genre** | Small **Genre** table (dropdown) vs single string on Book | Which do you prefer? |
| 5 | **Reading time** | Start with formula (PageCount/200 → hours), optional AI later | OK? |
| 6 | **Ease of reading** | One AI call per book (or on demand) storing Easy/Medium/Hard | OK? |
| 7 | **Notify on return** | In-app notifications table first; email optional | OK? |
| 8 | **SSO** | Microsoft + Google only | Add GitHub or others? |
| 9 | **Deploy target** | Railway or Azure App Service (both have free tier) | Which do you have an account for? |

Once you confirm these (or say “go with your suggestions”), we can **lock the structure** and start implementing step by step in .NET.

---

## 12. Locked decisions (implemented)

| Decision | Choice |
|----------|--------|
| **Structure** | **One project** — Blazor Web App + API in same project (single deploy). |
| **Database** | **SQL Server** (LocalDB for local dev; connection string in appsettings). |
| **Genre** | **Genre table** — seeded with Fiction, Non-Fiction, Science, History, Biography, Children, Other. |
| **Notify on return** | **Email** — `IEmailNotificationService` / `EmailNotificationService`; configure `SmtpSettings` in appsettings. |

Solution and folder structure have been created under `src/LibraryManagement` (see README and §8).
