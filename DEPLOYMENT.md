# Deploying Mini Library Management System for Public Web Access

This guide gets the app online with your **Neon (PostgreSQL)** database and makes it reachable on the public internet.

## Prerequisites

- **Database**: Neon (PostgreSQL) — you already have this. Use the connection string from the Neon dashboard (e.g. `Host=...;Database=...;Username=...;Password=...;SSL Mode=Require`).
- **Host**: A web host (e.g. **Render** or **Azure App Service**). The app runs in Docker on Render; on Azure it can run as a .NET 9 Web App.
- **Google OAuth**: Client ID and Client Secret for your **production URL** (add the deploy URL to Authorized redirect URIs and origins in Google Cloud Console).

## Quick path: Deploy with Neon

1. **Database**: Use your existing Neon database. Ensure migrations have been applied (run the app locally once with Neon, or run `dotnet ef database update` with `DatabaseProvider: PostgreSQL` in config).
2. **Host**: Choose one:
   - **Render** (free tier, Docker): See [Deploy to Render (with Neon)](#9-deploy-to-render-with-neon) below.
   - **Azure App Service**: See [Azure DevOps + Azure App Service](#7-azure-devops--azure-app-service-cicd); set `DatabaseProvider` = `PostgreSQL` and `ConnectionStrings__DefaultConnection` = your Neon connection string.
3. **Config**: Set connection string, `DatabaseProvider=PostgreSQL`, and Google OAuth via the host’s environment variables (no secrets in the repo).
4. **Google OAuth**: In Google Cloud Console, add `https://<your-deploy-url>/signin-google` and `https://<your-deploy-url>` (origin).
5. Open your public URL and sign in with Google.

## 1. Host and database

- **Host**: Choose a host (e.g. Azure App Service, Render, IIS). Ensure the host provides HTTPS (TLS certificate).
- **Database**: Use **Neon (PostgreSQL)** or SQL Server. The app runs migrations and seeds on startup; point the connection string to your database.

## 2. Production configuration (no secrets in repo)

Do **not** put production connection strings, OAuth secrets, or SMTP passwords in `appsettings.json` or commit them. Use the host’s **environment variables** or **secret store** (e.g. Azure App Settings, Key Vault).

### Environment variable names (ASP.NET Core convention)

Replace `__` with `:` when using JSON keys. Examples:

| Setting | Env var (example) |
|--------|---------------------|
| Connection string | `ConnectionStrings__DefaultConnection` |
| Google Client ID | `Authentication__Google__ClientId` |
| Google Client Secret | `Authentication__Google__ClientSecret` |
| Allowed hosts | `AllowedHosts` (semicolon-separated, e.g. `yourdomain.com;*.azurewebsites.net`) |
| Database provider | `DatabaseProvider` = `SqlServer` or `PostgreSQL` (default: SqlServer) |
| Data Protection key path | `DataProtection__KeyPath` (optional; e.g. `/home/data/keys`) |
| Forwarded Headers | `ForwardedHeaders__Enabled` = `true` (only when behind a reverse proxy) |

For **PostgreSQL** (e.g. Render): set `DatabaseProvider` to `PostgreSQL` and `ConnectionStrings__DefaultConnection` to your PostgreSQL connection string (e.g. `Host=...;Database=...;Username=...;Password=...;SSL Mode=Require`). The app supports both SQL Server (default) and PostgreSQL with the same codebase.

### Optional: `appsettings.Production.json`

You can add `src/MiniLibraryManagementSystem/appsettings.Production.json` on the server only (it is in `.gitignore`). Use it for non-secret production values; keep secrets in environment variables or the host’s secret store.

See `src/MiniLibraryManagementSystem/appsettings.Production.example.json` for the structure. Do **not** commit real values.

## 3. Google OAuth for the public URL

In **Google Cloud Console** (APIs & Services → Credentials → your OAuth 2.0 Client ID):

- **Authorized redirect URIs**: add  
  `https://<your-public-host>/signin-google`  
  (e.g. `https://yourapp.azurewebsites.net/signin-google`).
- **Authorized JavaScript origins** (if required): `https://<your-public-host>` (no trailing slash).

Keep existing localhost redirect URIs for local development.

## 4. HTTPS and reverse proxy

- The app uses `UseHttpsRedirection()` and `UseHsts()` in production. Ensure the host is configured for HTTPS.
- If the app runs **behind a reverse proxy or load balancer**, set `ForwardedHeaders:Enabled` to `true` (via config or env) so `Request.Scheme` and `Request.Host` are correct for OAuth redirects and links.

## 5. Data Protection (optional)

For a **single instance**, the default ephemeral key ring is fine. For **multiple instances** or to keep cookies/antiforgery valid across **app restarts**, set `DataProtection:KeyPath` to a directory path that is shared or persistent (e.g. a mounted volume). Only the app identity should have read/write access. Restrict this directory in production.

## 6. Deploy steps (high level)

1. Provision the host and database; obtain the production connection string and public URL.
2. Configure production settings (connection string, Google ClientId/ClientSecret, optional SMTP, AllowedHosts) via environment variables or the host’s secret store.
3. Add the production callback URL (and origin) to the Google OAuth client (step 3 above).
4. Publish: `dotnet publish -c Release -o ./publish` (from the project directory). Deploy the `publish` output to the host.
5. Open the public URL, sign in with Google, and verify core flows (e.g. add book, borrow, check-in).

## 7. Azure DevOps + Azure App Service (CI/CD)

The repo includes **`azure-pipelines.yml`** for Azure DevOps. It builds on every push to `main` or `develop` and deploys to Azure App Service when the pipeline runs from **`main`** (or when variable `DeployToAzure` is set to `true`).

### 7.1 Create the pipeline in Azure DevOps

1. In your project (**MiniLibraryManagementSystem**), go to **Pipelines** → **Pipelines**.
2. Click **Create pipeline** → choose your repo (e.g. **Azure Repos Git** or **GitHub**).
3. Select **Existing Azure Pipelines YAML file** and choose the **`azure-pipelines.yml`** in the root of the repo.
4. Click **Continue** and then **Save** (or **Run** to test). The **Build** stage will run and succeed. The **Deploy** stage is skipped until you create the Azure service connection and set the pipeline variables below (so you will not see the error "service connection ... could not be found" once the YAML condition is in place).

### 7.2 Azure resources

1. **Azure App Service (Web App)**  
   - In Azure Portal, create a **Web App** (e.g. **Linux**, runtime **.NET 9** or **Docker** if you use the project’s Dockerfile).  
   - Note the app name (e.g. `minilibrary-web`). The URL will be `https://<app-name>.azurewebsites.net`.

2. **Database**  
   - **Using Neon (PostgreSQL)**: Use your existing Neon connection string. No Azure SQL needed. In App Service configuration set `DatabaseProvider` = `PostgreSQL` and `ConnectionStrings__DefaultConnection` = your Neon URL.  
   - **Using Azure SQL**: Create a database and note the connection string; allow **Azure services** and/or the App Service outbound IPs in the firewall.

3. **Service connection in Azure DevOps**  
   - **Project settings** → **Service connections** → **New service connection** → **Azure Resource Manager**.  
   - Choose **Service principal (automatic)** or **Workload identity**, select your subscription and (optionally) resource group, name it (e.g. `Azure-App-Service-Connection`).  
   - Grant access to the App Service so the pipeline can deploy.

### 7.3 Pipeline variables

In the pipeline (**Edit** → **Variables** or **Pipeline** → **Edit** → **Variables**), add:

| Name | Value | Secret? |
|------|--------|--------|
| `AzureServiceConnection` | Name of the service connection (e.g. `Azure-App-Service-Connection`) | No |
| `AzureWebAppName` | Your Web App name (e.g. `minilibrary-web`) | No |

To deploy from a branch other than `main`, add:

| Name | Value |
|------|--------|
| `DeployToAzure` | `true` |

### 7.4 App Service configuration

In **Azure Portal** → your Web App → **Configuration** → **Application settings**, add (or use **Key Vault references**):

- `ConnectionStrings__DefaultConnection` = your **Neon** connection string (e.g. `Host=...;Database=...;Username=...;Password=...;SSL Mode=Require`) or your Azure SQL connection string.
- `DatabaseProvider` = `PostgreSQL` when using Neon; omit or set to `SqlServer` for Azure SQL.
- `Authentication__Google__ClientId` = your Google OAuth Client ID.
- `Authentication__Google__ClientSecret` = your Google OAuth Client Secret (mark as **Secret**).
- `AllowedHosts` = `*` or e.g. `yourapp.azurewebsites.net`.
- `ForwardedHeaders__Enabled` = `true` (recommended when behind Azure’s load balancer).

Then add the production URL to **Google Cloud Console** → your OAuth client → **Authorized redirect URIs**:  
`https://<your-app-name>.azurewebsites.net/signin-google`.

### 7.5 Run the pipeline

- Push to **`main`** (or to your branch with `DeployToAzure=true`) to trigger build and deploy.
- The first deploy will run EF migrations and seed data on app startup. Open `https://<your-app-name>.azurewebsites.net`, sign in with Google, and verify the app.

## 8. Optional: other CI/CD

Use GitHub Actions or similar to run `dotnet publish` and deploy to the host. Store secrets in the pipeline/host configuration and map them to the same setting names (e.g. App Service application settings).

## 9. Deploy to Render (with Neon)

Render runs the app in **Docker** and works well with your existing **Neon** database. The repo includes a **Dockerfile** and **`render.yaml`** (Blueprint).

### 9.1 One-time setup

1. **Render account**: Sign up at [render.com](https://render.com) (free tier is enough).
2. **Connect the repo**: Dashboard → **New** → **Blueprint**; connect your Git provider and select the repo that contains `render.yaml`. Render will detect the Blueprint.
3. **Create resources**: Click **Apply**. Render creates a web service from `render.yaml`. It will prompt for secrets (see below).
4. **Environment variables**: In the new web service → **Environment**, set:
   - `ConnectionStrings__DefaultConnection` = your **Neon** connection string (from Neon dashboard, e.g. `Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require`).
   - `Authentication__Google__ClientId` = your Google OAuth Client ID.
   - `Authentication__Google__ClientSecret` = your Google OAuth Client Secret.
   - `DatabaseProvider` and `AllowedHosts` / `ForwardedHeaders__Enabled` are already set in `render.yaml`; override if needed.
5. **Google OAuth**: In Google Cloud Console, add:
   - **Authorized redirect URIs**: `https://<your-service-name>.onrender.com/signin-google`
   - **Authorized JavaScript origins**: `https://<your-service-name>.onrender.com`  
   (Replace `<your-service-name>` with the name from Render, e.g. `minilibrary`.)

### 9.2 Deploy and run

- **First deploy**: Render builds the Docker image and runs the app. On startup the app applies EF migrations and seeds data. The app will be at `https://<your-service-name>.onrender.com`.
- **Later changes**: Push to the connected branch; Render redeploys automatically (if auto-deploy is on).

### 9.3 Manual create (without Blueprint)

If you prefer not to use the Blueprint: **New** → **Web Service** → connect repo → set **Root Directory** to `src/MiniLibraryManagementSystem`, **Runtime** to **Docker**. Add the same environment variables as above.

## Summary checklist

- [ ] **Database**: Neon is ready; migrations applied (run app locally with Neon once, or `dotnet ef database update` with PostgreSQL config).
- [ ] **Host**: Choose Render (section 9) or Azure App Service (section 7); get public URL.
- [ ] **Config**: Set `ConnectionStrings__DefaultConnection`, `DatabaseProvider=PostgreSQL`, and Google OAuth via host env vars (no secrets in repo).
- [ ] **Google OAuth**: Add production redirect URI and origin in Google Cloud Console.
- [ ] **HTTPS**: Use host’s default HTTPS; set `ForwardedHeaders__Enabled` = `true` when behind a proxy.
- [ ] **Deploy**: Deploy app; confirm migrations/seed on first start; test login and main flows.
- [ ] (Optional) Set DataProtection:KeyPath if scaling out or requiring cookie survival across restarts.
- [ ] (Optional) CI/CD: use **azure-pipelines.yml** (section 7) for Azure, or Render’s built-in deploys from Git (section 9).
