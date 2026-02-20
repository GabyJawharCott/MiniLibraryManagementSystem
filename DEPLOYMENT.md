# Deploying Mini Library Management System for Public Web Access

This checklist and configuration guide supports publishing the app so it is reachable on the public internet (e.g. Azure App Service, IIS, or other hosts).

## Prerequisites

- .NET 9 runtime on the host (or publish self-contained).
- SQL Server or Azure SQL Database.
- Google OAuth 2.0 credentials (Client ID and Client Secret) for the production URL.

## 1. Host and database

- **Host**: Choose a host (e.g. Azure App Service, IIS, or other). Ensure the host provides HTTPS (TLS certificate).
- **Database**: Create a SQL Server or Azure SQL database. The app runs migrations and seeds on startup, so point the connection string to an empty (or new) database.

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
| Data Protection key path | `DataProtection__KeyPath` (optional; e.g. `/home/data/keys`) |
| Forwarded Headers | `ForwardedHeaders__Enabled` = `true` (only when behind a reverse proxy) |

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
   - In Azure Portal, create a **Web App** (e.g. **Linux**, runtime **.NET 9**).  
   - Note the app name (e.g. `minilibrary-web`). The URL will be `https://<app-name>.azurewebsites.net`.

2. **Azure SQL Database** (or SQL Server)  
   - Create a database and note the **connection string**.  
   - In the database firewall, allow **Azure services** and/or the outbound IPs of the App Service if needed.

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

- `ConnectionStrings__DefaultConnection` = your Azure SQL connection string (mark as slot setting if you use deployment slots).
- `Authentication__Google__ClientId` = your Google OAuth Client ID.
- `Authentication__Google__ClientSecret` = your Google OAuth Client Secret (mark as **Secret**).
- `AllowedHosts` = `*` or e.g. `yourapp.azurewebsites.net`.

Then add the production URL to **Google Cloud Console** → your OAuth client → **Authorized redirect URIs**:  
`https://<your-app-name>.azurewebsites.net/signin-google`.

### 7.5 Run the pipeline

- Push to **`main`** (or to your branch with `DeployToAzure=true`) to trigger build and deploy.
- The first deploy will run EF migrations and seed data on app startup. Open `https://<your-app-name>.azurewebsites.net`, sign in with Google, and verify the app.

## 8. Optional: other CI/CD

Use GitHub Actions or similar to run `dotnet publish` and deploy to the host. Store secrets in the pipeline/host configuration and map them to the same setting names (e.g. App Service application settings).

## Summary checklist

- [ ] Choose host and database; get connection string and public URL.
- [ ] Set production config via env/host secrets (no secrets in repo).
- [ ] Add production redirect URI and origin in Google Cloud Console.
- [ ] Ensure HTTPS; enable Forwarded Headers only if behind a proxy.
- [ ] Deploy app; confirm migrations/seed on first start; test login and main flows.
- [ ] (Optional) Set DataProtection:KeyPath if scaling out or requiring cookie survival across restarts.
- [ ] (Optional) Add CI/CD: use **azure-pipelines.yml** in Azure DevOps (see section 7) for build and deploy to Azure App Service.
