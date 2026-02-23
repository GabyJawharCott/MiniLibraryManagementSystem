# Deploying Mini Library Management System for Public Web Access

This guide gets the app online with your **Neon (PostgreSQL)** database and makes it reachable on the public internet.

## Prerequisites

- **Database**: Neon (PostgreSQL) — you already have this. Use the connection string from the Neon dashboard (e.g. `Host=...;Database=...;Username=...;Password=...;SSL Mode=Require`).
- **Host**: A web host (e.g. **Render** or **Azure App Service**). The app runs in Docker on Render; on Azure it can run as a .NET 10 Web App.
- **Google OAuth**: Client ID and Client Secret for your **production URL** (add the deploy URL to Authorized redirect URIs and origins in Google Cloud Console).

## Quick path: Deploy with Neon

1. **Database**: Use your existing Neon database. Ensure migrations have been applied (run the app locally once with Neon, or run `dotnet ef database update` with `DatabaseProvider: PostgreSQL` in config).
2. **Host**: Choose one:
  - **Render** (free tier: create a Web Service manually, no Blueprint): See [Deploy to Render (with Neon)](#9-deploy-to-render-with-neon--free-tier) below.
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


| Setting                  | Env var (example)                                                               |
| ------------------------ | ------------------------------------------------------------------------------- |
| Connection string        | `ConnectionStrings__DefaultConnection`                                          |
| Google Client ID         | `Authentication__Google__ClientId`                                              |
| Google Client Secret     | `Authentication__Google__ClientSecret`                                          |
| Allowed hosts            | `AllowedHosts` (semicolon-separated, e.g. `yourdomain.com;*.azurewebsites.net`) |
| Database provider        | `DatabaseProvider` = `SqlServer` or `PostgreSQL` (default: SqlServer)           |
| Data Protection key path | `DataProtection__KeyPath` (optional; e.g. `/home/data/keys`)                    |
| Forwarded Headers        | `ForwardedHeaders__Enabled` = `true` (only when behind a reverse proxy)         |


For **PostgreSQL** (e.g. Render): set `DatabaseProvider` to `PostgreSQL` and `ConnectionStrings__DefaultConnection` to your PostgreSQL connection string (e.g. `Host=...;Database=...;Username=...;Password=...;SSL Mode=Require`). The app supports both SQL Server (default) and PostgreSQL with the same codebase.

### All environment variables and values


| Environment variable                      | Description                                      | Example value                                                                                                                                                                                                                                              | Required?                                                  |
| ----------------------------------------- | ------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| `ConnectionStrings__DefaultConnection`    | Database connection string                       | **Neon:** `Host=ep-xxx.neon.tech;Database=neondb;Username=neondb_owner;Password=YOUR_PASSWORD;SSL Mode=Require` **SQL Server:** `Server=(localdb)\mssqllocaldb;Database=MiniLibraryManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true` | Yes                                                        |
| `ConnectionStrings__AppendTrustServerCertificate` | Append `Trust Server Certificate=true` for Neon in Docker (fixes SSL handshake on Render) | `true` or omit | Set to `true` on Render if you see SslStream/SSL connection errors to Neon |
| `DatabaseProvider`                        | Which database to use                            | `PostgreSQL` or `SqlServer` (default)                                                                                                                                                                                                                      | Yes for Neon; omit or `SqlServer` for SQL Server           |
| `Authentication__Google__ClientId`        | Google OAuth 2.0 Client ID                       | `123456789-xxx.apps.googleusercontent.com`                                                                                                                                                                                                                 | Yes for Google sign-in                                     |
| `Authentication__Google__ClientSecret`    | Google OAuth 2.0 Client Secret                   | `GOCSPX-xxxxx`                                                                                                                                                                                                                                             | Yes for Google sign-in                                     |
| `Authentication__Microsoft__ClientId`     | Microsoft account Client ID                      | (optional)                                                                                                                                                                                                                                                 | No; only if using Microsoft sign-in                        |
| `Authentication__Microsoft__ClientSecret` | Microsoft account Client Secret                  | (optional)                                                                                                                                                                                                                                                 | No; only if using Microsoft sign-in                        |
| `AllowedHosts`                            | Allowed Host header values (semicolon-separated) | `*` or `yourapp.onrender.com;*.azurewebsites.net`                                                                                                                                                                                                          | Recommended in production; default `*` in appsettings      |
| `ForwardedHeaders__Enabled`               | Use X-Forwarded-* headers (behind proxy)         | `true` or `false`                                                                                                                                                                                                                                          | Yes when behind Render/Azure load balancer                 |
| `DataProtection__KeyPath`                 | Directory for persistent Data Protection keys    | `/home/data/keys` or leave empty                                                                                                                                                                                                                           | Optional; for scale-out or cookie survival across restarts |
| `SmtpSettings__Host`                      | SMTP server hostname                             | `smtp.gmail.com` or empty to disable email                                                                                                                                                                                                                 | Optional                                                   |
| `SmtpSettings__Port`                      | SMTP port                                        | `587`                                                                                                                                                                                                                                                      | Optional (default 587)                                     |
| `SmtpSettings__From`                      | From email address                               | `library@example.com`                                                                                                                                                                                                                                      | Optional (required if sending email)                       |
| `SmtpSettings__User`                      | SMTP username                                    | (e.g. Gmail address)                                                                                                                                                                                                                                       | Optional                                                   |
| `SmtpSettings__Password`                  | SMTP password / app password                     | (secret)                                                                                                                                                                                                                                                   | Optional                                                   |
| `SmtpSettings__EnableSsl`                 | Use SSL for SMTP                                 | `true` or `false`                                                                                                                                                                                                                                          | Optional (default true)                                    |
| `PORT`                                    | Port to listen on (set by Render/Railway)        | e.g. `10000`                                                                                                                                                                                                                                               | Set by host; app reads it automatically                    |
| `ASPNETCORE_ENVIRONMENT`                  | Environment name                                 | `Development`, `Production`, `Staging`                                                                                                                                                                                                                     | Usually set by host; default `Production` when not set     |


**Minimal set for Render + Neon + Google sign-in:**  
`DatabaseProvider`, `ConnectionStrings__DefaultConnection`, `ConnectionStrings__AppendTrustServerCertificate` = `true`, `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`, `AllowedHosts`, `ForwardedHeaders__Enabled`.

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

The repo includes `**azure-pipelines.yml`** for Azure DevOps. It builds on every push to `main` or `develop` and deploys to Azure App Service when the pipeline runs from `**main**` (or when variable `DeployToAzure` is set to `true`).

### 7.1 Create the pipeline in Azure DevOps

1. In your project (**MiniLibraryManagementSystem**), go to **Pipelines** → **Pipelines**.
2. Click **Create pipeline** → choose your repo (e.g. **Azure Repos Git** or **GitHub**).
3. Select **Existing Azure Pipelines YAML file** and choose the `**azure-pipelines.yml`** in the root of the repo.
4. Click **Continue** and then **Save** (or **Run** to test). The **Build** stage will run and succeed. The **Deploy** stage is skipped until you create the Azure service connection and set the pipeline variables below (so you will not see the error "service connection ... could not be found" once the YAML condition is in place).

### 7.2 Azure resources

1. **Azure App Service (Web App)**
  - In Azure Portal, create a **Web App** (e.g. **Linux**, runtime **.NET 10** or **Docker** if you use the project’s Dockerfile).  
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


| Name                     | Value                                                                | Secret? |
| ------------------------ | -------------------------------------------------------------------- | ------- |
| `AzureServiceConnection` | Name of the service connection (e.g. `Azure-App-Service-Connection`) | No      |
| `AzureWebAppName`        | Your Web App name (e.g. `minilibrary-web`)                           | No      |


To deploy from a branch other than `main`, add:


| Name            | Value  |
| --------------- | ------ |
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

- Push to `**main`** (or to your branch with `DeployToAzure=true`) to trigger build and deploy.
- The first deploy will run EF migrations and seed data on app startup. Open `https://<your-app-name>.azurewebsites.net`, sign in with Google, and verify the app.

## 8. Optional: other CI/CD

Use GitHub Actions or similar to run `dotnet publish` and deploy to the host. Store secrets in the pipeline/host configuration and map them to the same setting names (e.g. App Service application settings).

## 9. Deploy to Render (with Neon) — free tier

Render’s **free tier** lets you run one **Web Service** (no Blueprint required; Blueprints are a paid feature). The repo includes a **Dockerfile**; you create the service manually in the dashboard.

### 9.1 Create a Web Service (no payment)

1. **Render account**: Sign up at [render.com](https://render.com).
2. **New Web Service**: Dashboard → **New** → **Web Service** (not Blueprint).
3. **Connect the repo**: Connect your Git provider (GitHub, etc.) and select this repo. Use the branch you want to deploy (e.g. `main`).
4. **Configure the service**:
  - **Name**: e.g. `minilibrary`.
  - **Region**: Pick one (e.g. Frankfurt).
  - **Root Directory**: set to `**src/MiniLibraryManagementSystem`** (where the Dockerfile is).
  - **Runtime**: **Docker**.
  - **Instance type**: Free (if available in your region).
5. **Environment variables**: In the **Environment** tab, add:

  | Key                                    | Value                                                                                                    |
  | -------------------------------------- | -------------------------------------------------------------------------------------------------------- |
  | `DatabaseProvider`                     | `PostgreSQL`                                                                                             |
  | `ConnectionStrings__DefaultConnection` | Your Neon connection string (e.g. `Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require`) |
  | `ConnectionStrings__AppendTrustServerCertificate` | `true` (fixes Neon SSL in Docker) |
  | `Authentication__Google__ClientId`     | Your Google OAuth Client ID                                                                              |
  | `Authentication__Google__ClientSecret` | Your Google OAuth Client Secret                                                                          |
  | `AllowedHosts`                         | `*`                                                                                                      |
  | `ForwardedHeaders__Enabled`            | `true`                                                                                                   |

6. **Google OAuth**: In Google Cloud Console, add:
  - **Authorized redirect URIs**: `https://<your-service-name>.onrender.com/signin-google`
  - **Authorized JavaScript origins**: `https://<your-service-name>.onrender.com`  
   (Use the service name you gave in step 4, or the URL Render shows after the first deploy.)
7. Click **Create Web Service**. Render builds the Docker image and deploys. On first start the app runs migrations and seeds. Your app will be at `https://<your-service-name>.onrender.com`.

### 9.2 After the first deploy

- **Auto-deploy**: By default, pushes to the connected branch trigger a new deploy.
- **Secrets**: Never commit Neon or Google secrets; keep them only in Render’s Environment.

**Neon SSL error (SslStream.SendAuthResetSignal / connection to database 'neondb'):** Add env var **`ConnectionStrings__AppendTrustServerCertificate`** = **`true`** in Render (Environment). The app will then use SSL Mode=Require and skip server certificate validation when connecting to Neon (required in Docker where the CA store can cause handshake failures). If your connection string uses `SSL Mode=VerifyFull` or `VerifyCA`, the app will override it to `Require` when this option is set.

**Port scan timeout / no open ports:** The app binds to `PORT` and runs migrations in the background after startup, so Render should see the port. If you still get “no open ports”, ensure `PORT` is set by Render (it usually is) and that no env var overrides `ASPNETCORE_URLS` with a different port.

**Exit status 139 (crash/segfault):** The Dockerfile uses `linux/amd64` and `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` to reduce this. If it still happens on Render’s free tier (512 MB RAM), try upgrading to a paid instance with more memory, or add env var `DOTNET_GCHeapHardLimit` (e.g. `0x1E000000` for ~480 MB) to cap GC and avoid OOM kills that can look like 139.

**“An error occurred while processing your request” (with Request ID):** In Production the app hides exception details. To see the real error:
1. **Render Logs** — In Render Dashboard → your service → **Logs**, reproduce the error, then search for your Request ID (e.g. `1fbd00abdc01f6040877d727b369f6a8`) or for `Exception` or `fail:`. The full exception and stack trace are logged there.
2. **Temporarily show errors in the browser** — In Render → **Environment**, add **`ASPNETCORE_ENVIRONMENT`** = **`Development`**. Redeploy, reproduce the error; the page will show the exception details. **Remove this env var** (or set it back to `Production`) when done, so you don’t expose errors to users.

### 9.3 Optional: Blueprint

The repo includes `**render.yaml`** for Render Blueprints. Blueprints are a paid feature; you can ignore that file and use the manual Web Service steps above on the free tier.

## Summary checklist

- **Database**: Neon is ready; migrations applied (run app locally with Neon once, or `dotnet ef database update` with PostgreSQL config).
- **Host**: Choose Render (section 9) or Azure App Service (section 7); get public URL.
- **Config**: Set `ConnectionStrings__DefaultConnection`, `DatabaseProvider=PostgreSQL`, and Google OAuth via host env vars (no secrets in repo).
- **Google OAuth**: Add production redirect URI and origin in Google Cloud Console.
- **HTTPS**: Use host’s default HTTPS; set `ForwardedHeaders__Enabled` = `true` when behind a proxy.
- **Deploy**: Deploy app; confirm migrations/seed on first start; test login and main flows.
- (Optional) Set DataProtection:KeyPath if scaling out or requiring cookie survival across restarts.
- (Optional) CI/CD: use **azure-pipelines.yml** (section 7) for Azure, or Render’s built-in deploys from Git (section 9).

