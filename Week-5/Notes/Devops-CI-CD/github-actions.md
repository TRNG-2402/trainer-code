# GitHub Actions: Workflows, Jobs, Steps, and Runners

## Learning Objectives
- Identify what GitHub Actions is and where workflow files live in a repository.
- Read and write a workflow YAML file using the correct file structure.
- Distinguish the four levels of the workflow hierarchy: Workflow, Job, Step, and Action (the GHA equivalent of Stage > Job > Step > Task).
- Choose between GitHub-hosted runners and self-hosted runners.
- Configure triggers (push, pull_request, schedule, workflow_dispatch).
- Use environment variables, secrets, and workflow artifacts.
- Read a failed workflow run's logs to find the failing step.
- Apply least-privilege with the top-level `permissions:` block.
- Gate jobs and steps with `if:` expressions.
- Run the same job across multiple OS or runtime versions in parallel using `strategy.matrix`.

## Why This Matters

Yesterday you read what a CI/CD pipeline is in the abstract. Today you write one. GitHub Actions is the CI/CD service built into GitHub -- which means if your code is on GitHub, you already have a pipeline platform with no separate signup, no separate billing for small projects, and no separate UI to learn.

By the end of this reading you will be able to write the workflow you are about to ship as part of the Friday exercise: a YAML file that, on every push to `main` or every pull request, checks out the repo, sets up Node, builds the React app, and uploads the build output as an artifact. On Friday we add a quality-gate step (SonarCloud) on top of it.

## The Concept

### What GitHub Actions Is

GitHub Actions is a workflow automation platform tied to repository events. When something happens in the repo -- a push, a pull request, an issue comment, a release, a scheduled time -- GitHub can run a sequence of commands you have defined in a YAML file. The same engine handles continuous integration, continuous deployment, repository housekeeping, scheduled jobs, and one-off automations.

Two important properties:

- **Workflows live in the repo.** Workflow files sit at `.github/workflows/*.yml`. They are version-controlled with your code, reviewed in PRs, and tied to the commit that defines them.
- **Each run is on a fresh, ephemeral machine.** A "runner" is a VM (or container) provisioned for the workflow run, used, and then thrown away. State does not persist between runs unless you explicitly save it (artifacts, caches, external storage).

### File Location and Structure

Every workflow file is a YAML file in the `.github/workflows/` directory of the default branch. The filename is up to you (`ci.yml`, `build-and-test.yml`, `deploy-prod.yml`); GitHub picks up every `.yml` file in that directory.

Minimum file structure:

```yaml
name: CI                        # Optional display name shown in the Actions UI.

on:                              # Triggers: when does this workflow run?
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:                            # One or more jobs.
  build:                         # Job ID (used in dependencies).
    name: Build React App        # Display name.
    runs-on: ubuntu-latest       # The runner image.

    steps:                       # Steps run sequentially within the job.
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm install
      - run: npm run build
```

Save that as `.github/workflows/ci.yml`, push to a branch, open a PR -- the workflow runs and decorates the PR with a status check.

### The Hierarchy

GitHub Actions has four levels. The Azure Pipelines equivalent terms are in parentheses for translation, since you may see both vocabularies in the wild.

| Level | What it is | Cardinality | Azure Pipelines analog |
|---|---|---|---|
| **Workflow** | A single YAML file. The unit a trigger fires on. | 1 file = 1 workflow. A repo can have many workflows. | Pipeline |
| **Job** | A group of steps that runs on a single runner. | A workflow has 1+ jobs. | Stage + Job (collapsed) |
| **Step** | A single shell command (`run:`) or a single Action invocation (`uses:`). Runs sequentially within a job. | A job has 1+ steps. | Step |
| **Action** | A pre-built, reusable unit -- a versioned package of code published to the GitHub Marketplace or another repo. Invoked from a step with `uses:`. | A step uses 0 or 1 Action. | Task |

Two specific points worth pinning down:

- **Jobs run in parallel by default.** If a workflow has three jobs, all three start at the same time on three separate runners. To force order, declare `needs:` (covered below).
- **Steps run sequentially within a job.** Each step's working directory and environment carry over to the next step in the same job. Across jobs, nothing carries over -- jobs are isolated.

A worked example showing all four levels:

```yaml
name: Build, Test, and Package

on: [push, pull_request]

jobs:
  build-api:                              # Job 1
    name: Build the .NET API
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4         # Step 1 -> uses an Action
      - uses: actions/setup-dotnet@v4     # Step 2 -> uses an Action
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore               # Step 3 -> runs a shell command
      - run: dotnet build --no-restore    # Step 4
      - run: dotnet test --no-build       # Step 5

  build-web:                              # Job 2 -- runs in parallel with build-api
    name: Build the React App
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm install
      - run: npm run build

  package:                                # Job 3 -- waits for both builds
    name: Publish artifacts
    runs-on: ubuntu-latest
    needs: [build-api, build-web]         # Sequencing: wait for these jobs
    steps:
      - run: echo "Both builds passed; produce a release artifact."
```

Read the YAML against the table: one workflow file, three jobs (two parallel, one dependent), each job several steps, each step either `uses:` an Action or `run:`s a shell command.

### Runners

A runner is the machine that executes a job. You have two options.

**GitHub-hosted runners.** GitHub provisions a clean VM for each job, runs your steps, and tears it down. Three OS families: Ubuntu (`ubuntu-latest`), Windows (`windows-latest`), and macOS (`macos-latest`). The images come pre-installed with common tools (Node, .NET, Python, Docker, Git, the major language SDKs). For public repositories, they are free; for private repositories, they cost minutes from the account's GitHub Actions quota.

GitHub-hosted runners are the right default for every project that does not have a specific reason to choose otherwise.

**Self-hosted runners.** You install the runner agent on your own machine (a VM in your cloud, an on-prem server, even a laptop). When a workflow needs a self-hosted runner, GitHub queues the job and your machine picks it up.

You choose self-hosted when you need:

- Access to internal networks (build artifacts that must reach a private package feed, deploys to a private cluster).
- Hardware GitHub does not offer (large RAM, GPU, specific architecture).
- Caching beyond the GitHub-hosted ephemeral cache, e.g., a warm Docker layer cache shared across runs.
- Compliance constraints that prohibit code from running on third-party infrastructure.

Self-hosted runners come with operational cost: you patch them, you scale them, you secure them. Default to GitHub-hosted unless you have a concrete need.

### Triggers

Triggers are declared under the top-level `on:` key. The most common ones:

| Trigger | When it fires |
|---|---|
| `push` | A commit is pushed. Filter with `branches:`, `paths:`, `tags:`. |
| `pull_request` | A PR is opened, updated, reopened, or closed. Filter with `branches:`, `types:`. |
| `schedule` | A cron expression. UTC-only. Useful for nightly builds, scheduled scans. |
| `workflow_dispatch` | Manually triggered from the Actions tab. Useful for deploys you want to run on-demand. |
| `release` | A release is published in the repo. |
| `issues`, `issue_comment`, `pull_request_review`, etc. | Repo activity events; useful for automation bots. |

Examples:

```yaml
on:
  push:
    branches: [main]
    paths:
      - 'src/**'                # Only trigger on changes inside src/
      - '.github/workflows/**'

  pull_request:
    branches: [main]

  schedule:
    - cron: '0 6 * * *'         # 06:00 UTC daily

  workflow_dispatch:            # Manual "Run workflow" button
    inputs:
      environment:
        description: 'Target environment'
        required: true
        default: 'staging'
        type: choice
        options: [staging, production]
```

### Variables, Secrets, and Artifacts

**Environment variables.** Defined at workflow, job, or step scope under `env:`. Values are plain text and visible in logs.

```yaml
env:
  NODE_VERSION: '20'

jobs:
  build:
    runs-on: ubuntu-latest
    env:
      BUILD_CONFIG: Release
    steps:
      - run: echo "Node ${{ env.NODE_VERSION }}, config ${{ env.BUILD_CONFIG }}"
```

**Secrets.** Sensitive values (API tokens, deploy credentials, SonarCloud token) are stored at the repository, environment, or organization level in `Settings -> Secrets and variables -> Actions`. They are masked in logs and never exposed in workflows triggered from forks unless you explicitly opt in.

```yaml
steps:
  - run: ./deploy.sh
    env:
      AZURE_DEPLOY_TOKEN: ${{ secrets.AZURE_DEPLOY_TOKEN }}
```

**Artifacts.** Files produced by a job and saved to GitHub for download by humans or downstream jobs. The retention defaults to 90 days (configurable per repo).

```yaml
steps:
  - run: npm run build
  - uses: actions/upload-artifact@v4
    with:
      name: web-dist
      path: dist/

# In a later job that needs the artifact:
  - uses: actions/download-artifact@v4
    with:
      name: web-dist
      path: dist/
```

Use artifacts to pass build output between jobs, or to make build output downloadable from the Actions UI for inspection.

### Securing the Pipeline: Permissions

Every workflow run uses the built-in `GITHUB_TOKEN` to interact with the repo (post status checks, comment on PRs, write packages). By default that token has broad write access. The right discipline is **least privilege** — declare only the scopes the workflow actually needs.

Set permissions at workflow scope (applies to every job) or per job:

```yaml
# Workflow-level: lock everything to read-only by default.
permissions:
  contents: read

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: npm ci && npm test

  comment-on-pr:
    runs-on: ubuntu-latest
    # Job-level: this job needs write access to PRs.
    permissions:
      contents: read
      pull-requests: write
    steps:
      - run: echo "post a comment via gh CLI"
```

Common scopes: `contents`, `pull-requests`, `issues`, `actions`, `packages`, `id-token`, `security-events`. Each can be `read`, `write`, or `none`. Combine permissions with secrets stored in `Settings -> Secrets and variables -> Actions`, plus protected environments for production deploy approvals — that triplet (secrets + permissions + environments) is what "secure pipeline configuration" means in practice.

### Conditional Execution: `if:`

`if:` expressions gate whether a job or step runs. Use them to skip work that does not apply, restrict deploys to specific branches, or react to previous step results.

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: npm ci
      - run: npm run build

      # Step-level: only upload coverage on main.
      - name: Upload coverage
        if: github.ref == 'refs/heads/main'
        run: ./scripts/upload-coverage.sh

      # Step-level: run even if a prior step failed.
      - name: Always print diagnostics
        if: always()
        run: cat build.log

  deploy:
    runs-on: ubuntu-latest
    needs: build
    # Job-level: only run on push to main, not on PRs.
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    steps:
      - run: ./deploy.sh
```

Useful built-in functions inside `if:` — `success()` (default), `always()`, `failure()`, `cancelled()`. Common context fields — `github.ref`, `github.event_name`, `github.actor`, `github.repository`.

### Running the Same Job Across Versions: `strategy.matrix`

`strategy.matrix` runs the job once for each combination of values you list — in parallel. The canonical use is testing against multiple Node/Python/.NET versions or multiple OS images.

```yaml
jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      fail-fast: false                  # let other matrix jobs finish on a failure
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        node: ['18', '20', '22']
        # Optional: drop a specific combo.
        exclude:
          - os: macos-latest
            node: '18'

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: ${{ matrix.node }}
      - run: npm ci
      - run: npm test
```

That config produces 8 jobs (3 OS x 3 Node minus the 1 excluded), each on its own runner. The Actions UI groups them under one workflow run and shows pass/fail per combination.

### Reading Run Logs to Debug a Failed Workflow

When a workflow fails, the Actions tab is your stack trace. The drill is the same every time:

1. **Open the failed run.** Repository -> Actions tab -> click the run with the red X. The run page lists every job in the workflow and shows which one failed.
2. **Open the failed job.** Click the failing job in the left pane. You see the ordered list of steps. The first step with a red X is your culprit; everything below it was skipped or did not run.
3. **Expand the failing step.** Click the step header to expand its log. The log is the raw stdout/stderr of the command — the same output you would have seen running it locally. Read from the bottom up: the last error message is usually the root cause; everything above is context.
4. **Search the log.** Each log section has a search box (or use Ctrl/Cmd+F). Search for `error`, `Error`, the test name, the file path, or the exit code.
5. **Re-run with debug logging if needed.** If the error is opaque, click "Re-run jobs -> Re-run with debug logging" (or set the `ACTIONS_STEP_DEBUG` and `ACTIONS_RUNNER_DEBUG` repo secrets to `true`). The re-run prints expanded internal diagnostics — useful when a step is failing inside a third-party Action you cannot read.
6. **Download artifacts when relevant.** If the failing step uploaded a log file, test report, or screenshot via `actions/upload-artifact`, the artifact appears at the bottom of the run page. Download and inspect.
7. **Compare to a working run.** If "the same workflow worked yesterday," open yesterday's run and diff the logs of the same step. The change is usually obvious — a new dependency, a runner image upgrade, a flaky integration.

Treat workflow logs as you treat any production log: scan for the first error, not the last. The first failure usually causes the cascade.

### Status Checks and Branch Protection

Every workflow run on a PR shows up as a status check on the PR. Once you pair this with the branch protection rules from the previous reading -- specifically *"Require status checks to pass before merging"* -- the workflow becomes a gate. A failing build literally blocks the merge button. This is the integration that makes a CI workflow valuable: the workflow is no longer advisory; it is enforced.

## Code Example: Real Workflow for a React App

This is the workflow you will ship as part of the Thursday/Friday exercises. Save as `.github/workflows/ci.yml`:

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    name: Build React App
    runs-on: ubuntu-latest

    steps:
      - name: Check out source
        uses: actions/checkout@v4

      - name: Set up Node.js 20
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build

      - name: Upload build output
        uses: actions/upload-artifact@v4
        with:
          name: web-dist
          path: dist/
          retention-days: 7
```

What this does, step by step:

1. Triggers on every push to `main` and every pull request targeting `main`.
2. Spins up an Ubuntu VM (`ubuntu-latest`).
3. Checks out the repository at the commit that triggered the run.
4. Installs Node.js 20 and enables npm cache (so subsequent runs reuse downloaded packages).
5. Runs `npm ci` -- the lockfile-respecting, CI-safe install.
6. Runs the build script defined in `package.json`.
7. Uploads the `dist/` directory as a downloadable artifact (kept for 7 days).

Once branch protection on `main` requires this workflow to pass, no PR can be merged unless the build is green.

## Summary

- GitHub Actions runs YAML workflows from `.github/workflows/*.yml` in response to repo events.
- Hierarchy: Workflow -> Job -> Step -> (optional) Action. Jobs run in parallel by default; steps are sequential within a job.
- Runners are the machines that execute jobs. Default to GitHub-hosted; choose self-hosted only for specific needs.
- Triggers (`push`, `pull_request`, `schedule`, `workflow_dispatch`) decide when a workflow runs.
- Secrets are stored in repo settings and injected via `${{ secrets.NAME }}`. Artifacts move build output between jobs or out to humans.
- Lock down `GITHUB_TOKEN` with a least-privilege `permissions:` block at workflow or job scope.
- `if:` gates jobs and steps on branch, event, or prior-step results — useful for branch-targeted deploys.
- `strategy.matrix` runs the same job in parallel across OS / runtime version combinations.
- Debug a failed run by opening the failed job, expanding the failing step, and reading the log bottom-up; re-run with debug logging when the error is opaque.
- Workflows become real quality gates when paired with branch protection requiring status checks to pass.

## Additional Resources

- [GitHub Docs: Workflow syntax for GitHub Actions](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [GitHub Docs: About GitHub-hosted runners](https://docs.github.com/en/actions/using-github-hosted-runners/about-github-hosted-runners)
- [GitHub Marketplace: Actions](https://github.com/marketplace?type=actions)
