# Upload code to GitHub (MiniLibraryManagementSystem)

Git is initialized and a `.gitignore` is in place. Follow these steps:

---

## 1. Add your GitHub remote

Replace **YOUR_GITHUB_USERNAME** with your GitHub username:

```powershell
cd "c:\Users\GabyJawhar\source\Cursor\Assessments\Assessment2"

git remote add origin https://github.com/YOUR_GITHUB_USERNAME/MiniLibraryManagementSystem.git
```

If your repo URL is different (e.g. under an organization), use that URL instead.

---

## 2. Stage all files

```powershell
git add .
```

---

## 3. Commit

```powershell
git commit -m "Initial commit: Mini Library Management System (.NET 9)"
```

---

## 4. Push to GitHub

For the **main** branch (default on GitHub):

```powershell
git branch -M main
git push -u origin main
```

If your default branch is **master**:

```powershell
git push -u origin master
```

---

## 5. Authenticate (if prompted)

- **HTTPS:** GitHub may ask for your username and a **Personal Access Token** (not your password). Create one at: GitHub → Settings → Developer settings → Personal access tokens.
- **SSH:** If you use SSH, set the remote to: `git@github.com:YOUR_GITHUB_USERNAME/MiniLibraryManagementSystem.git` and use that for `git push`.

---

## Quick copy-paste (after setting the remote)

```powershell
cd "c:\Users\GabyJawhar\source\Cursor\Assessments\Assessment2"
git add .
git commit -m "Initial commit: Mini Library Management System (.NET 9)"
git branch -M main
git push -u origin main
```
