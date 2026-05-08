# GitHub Overview: Repos, Issues, Projects, and Branch Protection

## Learning Objectives
- Identify the core GitHub product surfaces a project uses day-to-day: Repositories, Issues, Pull Requests, Projects, and Actions.
- Create and clone a repository, work on a feature branch, and open a pull request.
- Choose a branching strategy and explain why most teams default to trunk-based development with short-lived feature branches.
- Configure branch protection rules to require reviews and passing status checks before merge.
- Link Issues and Projects to commits and pull requests for end-to-end traceability.

## Why This Matters

Yesterday you learned what CI/CD is conceptually. Today you put the source-control and work-tracking foundations underneath it. Without those, a pipeline has nothing to fire on and no audit trail to attach to. GitHub bundles all the pieces a small or mid-size team needs: code hosting, issue tracking, planning boards, code review, packages, and CI/CD (Actions, covered in the next reading). Knowing how the pieces connect is what turns "I can `git push`" into "I can run an engineering team's day-to-day workflow."

This connects to the Week 4 epic directly: by Friday you will have a React app in a GitHub repository, a workflow that builds it on every push, branch protection that blocks unreviewed merges, and a quality gate (SonarCloud) that decorates pull requests with findings. All of that lives on GitHub.

## The Concept

### What GitHub Is

GitHub is a SaaS product built on top of Git. Git is the version control system you already use locally. GitHub is the hosted server where your repository lives, plus a layer of collaboration features around it: pull requests, code review, issues, project boards, automation, packages, and security scanning.

The major product surfaces, in the order you typically encounter them on a project:

| Surface | What it is | When you use it |
|---|---|---|
| **Repositories** | Hosted Git repos with a web UI for browsing files, history, and diffs. | All the time. The repo is the unit of project. |
| **Issues** | Lightweight tracked work items: bugs, features, questions, tasks. | When you find a bug, plan a feature, or want a discussion thread tied to a deliverable. |
| **Pull Requests (PRs)** | A proposal to merge one branch into another, with diff view, review comments, and required checks. | Every code change you want to land in a shared branch. |
| **Projects** | Customizable boards (table, board, roadmap views) that aggregate Issues and PRs across one or many repos. | Sprint planning, backlog management, cross-repo coordination. |
| **Actions** | The built-in CI/CD service. (Covered in detail in the next reading.) | Build, test, deploy, and any other automation triggered by repo events. |
| **Packages** | Hosted artifact registry: npm, NuGet, Maven, Docker images, etc. | Publishing reusable libraries or container images produced by your pipeline. |
| **Releases** | Tagged, downloadable bundles of your code with release notes. | Marking a milestone version (v1.0.0) and distributing binaries. |
| **Wiki / Discussions** | Long-form documentation and community Q&A. | Architecture docs, RFCs, open-ended discussion that does not belong in an Issue. |

For most enterprise .NET / full-stack teams, the day-to-day surfaces are Repositories, Issues (or Projects on top of Issues), Pull Requests, and Actions. The rest are situational.

### Repositories: Create and Clone

A repository is created either through the GitHub web UI (`New repository` button) or via the `gh` CLI. The minimum decisions at creation time:

- **Owner.** A user account or an organization.
- **Visibility.** Public (anyone on the internet can read) or private (only members you grant access). For client work and internal training projects, default to private.
- **Initial files.** README, `.gitignore` template (Visual Studio for .NET, Node for React), license. Initializing with at least a README means the repo has a default branch you can clone immediately.

To work locally:

```bash
git clone https://github.com/<owner>/<repo>.git
cd <repo>
git checkout -b feature/add-product-card
# ...edit files...
git add .
git commit -m "Add ProductCard component"
git push -u origin feature/add-product-card
```

The `-u` flag on the first push sets up tracking so subsequent pushes are bare `git push`.

### Branching Strategies

A branching strategy is the team's agreement about which branches exist, what each is for, and how changes flow between them. The two strategies you will encounter most often:

- **Trunk-based development with short-lived feature branches.** There is one long-lived branch (`main`). Developers branch off it, do a small piece of work in a day or two, open a PR, get it reviewed, merge back to `main`. `main` is always deployable. This is the default for most teams shipping continuously.
- **Git Flow.** Multiple long-lived branches: `main` (production), `develop` (integration), `feature/*`, `release/*`, `hotfix/*`. More ceremony, more merge overhead, often more friction than benefit. Appropriate for projects with strict versioned releases (boxed software, mobile apps with app-store gates) but heavy for continuously deployed web apps.

Default to trunk-based with short-lived branches unless you have a concrete reason not to. The reason: branches that live a week or longer accumulate merge conflicts and hide work-in-progress from the rest of the team.

### Pull Requests

A pull request is the unit of code review on GitHub. A PR is a proposal to merge a source branch (your feature branch) into a target branch (usually `main`). The PR view contains:

- **Conversation tab.** Description, comments, review approvals, status checks (CI runs), linked Issues.
- **Commits tab.** The commits on the source branch.
- **Files changed tab.** The diff. Reviewers leave inline comments on specific lines.
- **Checks tab.** Status of every workflow run triggered by the PR. Green checkmark = pass; red X = failure.

Typical lifecycle:

1. Push a feature branch.
2. Open a PR (`gh pr create`, or via the web UI).
3. CI runs automatically. The PR shows pending checks while it runs.
4. Reviewers leave comments. You push fixups; CI re-runs.
5. Once approved and all checks pass, you merge. The default merge style is "Create a merge commit"; teams that prefer linear history use "Squash and merge" or "Rebase and merge."
6. The feature branch is deleted (GitHub offers a one-click delete after merge).

### Branch Protection

Branch protection rules are repository-level rules that block unsafe merges to a specific branch (almost always `main`). Without them, anyone with push access can `git push` directly to `main` and bypass review entirely. Common rules:

| Rule | Effect |
|---|---|
| **Require a pull request before merging** | No direct pushes to `main`; all changes must go through a PR. |
| **Require approvals (e.g., 1 or 2 reviewers)** | The PR cannot be merged until N people have submitted an Approved review. |
| **Dismiss stale approvals on new commits** | If you push more code after an approval, the approval is invalidated; reviewer must re-approve. |
| **Require status checks to pass before merging** | The PR cannot be merged until selected workflows (e.g., the build workflow) report success. |
| **Require branches to be up to date before merging** | The PR's source branch must include the latest `main` before merge. Prevents "passed CI on stale code, broke after merge." |
| **Require conversation resolution before merging** | All PR comments marked as needing resolution must be resolved. |
| **Require signed commits** | Commits must be GPG- or SSH-signed. |
| **Restrict who can push to matching branches** | Only specified users/teams can push (only relevant when you allow direct push -- usually you do not). |
| **Do not allow bypassing the above settings** | Even repo admins must follow the rules. Recommended: turn this on. |

For the Friday exercise, the minimum branch protection on `main` will be:

- Require a pull request.
- Require at least one approval.
- Require the build workflow status check to pass.

That alone prevents the most common quality regressions: unreviewed code on main and broken builds on main.

### Issues and Projects

**Issues** are work items scoped to a single repository. An Issue has a title, a description (Markdown), a state (open/closed), and any number of labels, assignees, and milestones. Issues serve as bugs, features, tasks, and discussion threads alike -- the difference is convention and labeling, not a separate type.

**Projects** are planning surfaces that span one or many repositories. A Project is a collection of Issues and PRs displayed in a chosen view: a board (Kanban columns: To Do / In Progress / Done), a table (sortable spreadsheet), or a roadmap (timeline). You add custom fields (priority, sprint, story points) and group / filter by them.

Projects do not own work items; they reference Issues and PRs. The same Issue can appear on multiple Projects.

A common setup:

- One Project per team or initiative.
- Columns: Backlog -> Ready -> In Progress -> In Review -> Done.
- Custom fields: Priority (P1/P2/P3), Sprint (Sprint 23, Sprint 24).
- Issues are added to the Project at planning time, dragged across columns as work progresses.

### Linking Work to Code

Traceability is the property that, given a line of production code, you can trace it back through the PR that introduced it, to the commit that introduced it, to the Issue it was solving, to the planning conversation it came from. GitHub gives you this for free if you use a few conventions.

- **Closes / Fixes / Resolves keywords.** A PR description containing `Closes #42` automatically closes Issue #42 when the PR is merged. Use these in PR descriptions, not commit messages.
- **#NNN references.** Mentioning `#42` anywhere -- in a commit message, PR comment, or Issue comment -- creates a back-link visible on Issue #42. Useful when a single Issue is touched by multiple PRs.
- **Project automation.** A Project can auto-move an Issue from "In Progress" to "Done" when a referencing PR is merged.

The result: from any commit, you can click through to the PR that contained it; from the PR, you can see the Issue and the Project it belonged to; from the Issue, you can see every PR that referenced it. That is the audit trail.

## Code Example: Day-One Workflow

A complete walkthrough of starting work on a new task. Assumes you have `gh` (the GitHub CLI) and `git` configured.

```bash
# 1. Pick up Issue #42 from the Project board.
# 2. Pull the latest main.
git checkout main
git pull origin main

# 3. Branch off main with a descriptive name.
git checkout -b feature/42-product-card

# 4. Do the work and commit in small chunks.
git add src/components/ProductCard.tsx src/components/ProductCard.module.css
git commit -m "Add ProductCard component (refs #42)"

# 5. Push and open a PR. The body uses 'Closes #42' so the Issue auto-closes on merge.
git push -u origin feature/42-product-card
gh pr create \
  --title "Add ProductCard component" \
  --body "Closes #42

Adds a styled ProductCard for the product list view."

# 6. CI runs automatically. Watch the status.
gh pr checks

# 7. After approval and green checks, merge.
gh pr merge --squash --delete-branch
```

After this sequence: Issue #42 is closed, the PR is merged into `main`, the feature branch is deleted, and the Project board moves the card to Done (assuming Project automation is configured).

## Summary

- GitHub is Git hosting plus a collaboration layer: Repos, Issues, PRs, Projects, Actions, Packages, Releases.
- Default to trunk-based development with short-lived feature branches and frequent PRs.
- Pull requests are the unit of code review and the trigger for CI.
- Branch protection on `main` is non-negotiable for any shared repository: require a PR, at least one approval, and passing status checks.
- Issues live in a single repo; Projects aggregate Issues and PRs across repos for planning.
- Conventions like `Closes #NNN` give you end-to-end traceability from planning to production code at no extra cost.

## Additional Resources

- [GitHub Docs: About pull requests](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests)
- [GitHub Docs: About protected branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches)
- [GitHub Docs: About Projects](https://docs.github.com/en/issues/planning-and-tracking-with-projects/learning-about-projects/about-projects)
