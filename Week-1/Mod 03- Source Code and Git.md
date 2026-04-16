# Module 3: Source Control & Git

---

## 3.1 Source Control Management

**Version Control Systems (VCS)** track changes to files over time — who changed what, when, and why.

**Three models:**

| Model | How It Works | Examples |
|-------|-------------|---------|
| Local VCS | Copies of folders (`v1`, `v2_final`) | Manual backups |
| Centralized (CVCS) | Single server, developers check out/commit | SVN, Perforce |
| Distributed (DVCS) | Every developer has a full copy of the repo | **Git**, Mercurial |

**Git is a DVCS** — the industry standard. You can work offline, branch cheaply, and every clone is a full backup.

---

## 3.2 Git Introduction

**Git** is a free, open-source DVCS created by Linus Torvalds (2005).

**Git vs. GitHub:**

- **Git** — the version control tool on your machine
- **GitHub** — a hosting platform that stores Git repos remotely and adds collaboration features (pull requests, issues)

### Setup

```bash
git --version
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

---

## 3.3 Git Fundamentals

Git tracks files across three local areas:

```
Working Directory    →    Staging Area    →    Repository
   (edit files)         (git add)            (git commit)
```

**File states:**

| State | Meaning |
|-------|---------|
| Untracked | Git doesn't know about this file |
| Modified | Changed since last commit, not staged |
| Staged | Marked for the next commit |
| Committed | Saved in Git's history |

### Essential Commands

```bash
git status                  # Check file states
git add filename.cs         # Stage a file
git add .                   # Stage all changes
git commit -m "message"     # Commit staged changes
git log                     # View history
git log --oneline           # Compact history
```

---

## 3.4 Initializing a Repository

**Create a new repo:**

```bash
cd MyProject
git init               # Creates hidden .git folder
git add .              # Stage everything
git commit -m "Initial commit"
```

**Clone an existing repo:**

```bash
git clone https://github.com/username/repo-name.git
cd repo-name
```

---

## 3.5 Gitignore

The `.gitignore` file tells Git which files to ignore.

```gitignore
# Build output
bin/
obj/

# IDE files
.vs/
.vscode/
*.user

# OS files
.DS_Store
Thumbs.db

# Secrets
appsettings.Development.json
*.env
```

**Generate a .NET-specific .gitignore:**

```bash
dotnet new gitignore
```

**Pattern syntax:**

| Pattern | Meaning |
|---------|---------|
| `*.log` | All `.log` files |
| `bin/` | Entire `bin` directory |
| `!important.log` | Exception — don't ignore this file |
| `**/temp` | Any `temp` folder at any depth |

> `.gitignore` only affects **untracked** files. To stop tracking an already-committed file: `git rm --cached filename`

---

## 3.6 Branching, Merging, Push & Pull

### Commits

Each commit = one logical change with a clear message.

```bash
# Good messages
git commit -m "Add user login validation"
git commit -m "Fix null reference in OrderService"

# Bad messages
git commit -m "stuff"
git commit -m "fixed it"
```

### Branching

```bash
git checkout -b feature/add-greeting   # Create and switch
git branch                             # List branches
git checkout main                      # Switch branch
```

Use prefixes: `feature/`, `bugfix/`, `hotfix/`

### Merging

```bash
git checkout main                      # Switch to target branch
git merge feature/add-greeting         # Merge feature into main
```

**Merge conflicts** occur when two branches change the same line:

```
<<<<<<< HEAD
Console.WriteLine("Hello from main!");
=======
Console.WriteLine("Hello from feature!");
>>>>>>> feature/add-greeting
```

Resolve by choosing the correct code, removing the markers, then committing.

### Push and Pull

```bash
git push origin main    # Upload commits to remote
git pull origin main    # Download + merge remote commits
```

---

## 3.7 Pushing to a Remote Repository

1. Create a new repo on GitHub (don't initialize with README if you have local commits)
2. Link your local repo:

```bash
git remote add origin https://github.com/username/MyProject.git
git remote -v                    # Verify
git push -u origin main          # Push + set upstream tracking
```

After `-u`, future pushes only need `git push`.

### Daily Workflow

```bash
# 1. Make changes
# 2. Stage
git add .
# 3. Commit
git commit -m "Describe what changed"
# 4. Push
git push
```

---

## Quick Reference

| Command | What It Does |
|---------|-------------|
| `git init` | Initialize a new repo |
| `git clone <url>` | Copy a remote repo |
| `git status` | Check file states |
| `git add .` | Stage all changes |
| `git commit -m "msg"` | Commit staged changes |
| `git log --oneline` | View compact history |
| `git checkout -b <name>` | Create + switch branch |
| `git merge <branch>` | Merge branch into current |
| `git push` | Upload to remote |
| `git pull` | Download from remote |
| `git remote -v` | List remotes |

---

## Key Takeaways

- Git is a **distributed** version control system — every clone is a full backup
- The workflow: **edit → stage (`git add`) → commit → push**
- **Branches** isolate work; **merge** brings it together
- Always set up `.gitignore` before your first push
- Write clear, descriptive commit messages
