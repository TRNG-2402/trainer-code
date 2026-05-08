# SonarCloud: Static Analysis and Quality Gates

## Learning Objectives
- Define static analysis and distinguish it from dynamic analysis.
- Identify what static analysis catches: code smells, bugs, security vulnerabilities, duplications, and complexity hotspots.
- Distinguish SonarCloud from SonarQube and choose the right one for a given context.
- Set up a SonarCloud project against a GitHub repository: organization, project key, and the GitHub App.
- Define a quality gate, recognize the default conditions, and explain why "new code" thresholds matter more than "overall code" thresholds.
- Add SonarCloud analysis to a GitHub Actions workflow and decorate pull requests with findings.

## Why This Matters

You have a pipeline. It builds. It runs tests. That tells you the code compiles and that the tests you wrote pass. It does not tell you whether the code is *good*. It will not flag a method with cyclomatic complexity of 40, a SQL injection sink, a duplicated 30-line block, or a hardcoded credential. Those problems escape unit tests because they are not behavioral; they are structural and security concerns.

Static analysis is the layer that catches them. SonarCloud is the most common static-analysis service in enterprise .NET / full-stack teams, and it integrates cleanly with both GitHub and GitHub Actions. By the end of today's exercise, every pull request to your project will be automatically scanned, the findings will be decorated as inline comments on the PR, and a "quality gate" status check will block the merge if the scan fails. That is the final piece of the Week 4 epic: build + test + quality, end to end, with no human in the loop until something is actually wrong.

## The Concept

### Static Analysis vs Dynamic Analysis

**Static analysis** examines the *source code itself* without executing it. The analyzer parses your code into an abstract syntax tree (and often a control-flow graph), walks it, and matches against rules: "this `if (a = b)` is probably an assignment where you meant comparison," "this method has 87 lines and should probably be split," "this string concatenation in a SQL query is a SQL-injection risk."

**Dynamic analysis** examines the *running program*: profilers, fuzzers, web vulnerability scanners (DAST tools like OWASP ZAP), runtime memory checkers. Dynamic analysis catches what only happens at runtime, but it requires the program to be running and exercised against representative input.

The two are complementary. Static analysis is fast, deterministic, and finds whole categories of problems before the code ever runs. It cannot find bugs that only manifest under specific runtime conditions. Dynamic analysis fills that gap but is slower and only finds what the test inputs exercise.

CI/CD pipelines almost always include static analysis (cheap, fast, runs on every commit) and sometimes include dynamic analysis as a longer scheduled job.

### What Static Analysis Catches

A typical static analyzer reports findings in several categories:

| Category | Example finding |
|---|---|
| **Bugs** | Likely defects: dereferencing a possibly-null reference, unreachable code, off-by-one indexing, unused return values from `Task`-returning methods. |
| **Code smells** | Maintainability problems that are not bugs but slow future work: methods >50 lines, classes >500 lines, deeply nested control flow, magic numbers, dead code. |
| **Security vulnerabilities** | Specific known-dangerous patterns: SQL injection sinks, hardcoded credentials, weak cryptography, XSS sinks, insecure deserialization. |
| **Security hotspots** | Code that *could* be a vulnerability and needs a human to confirm. Lower confidence than a vulnerability finding; surfaces patterns like "this code uses random number generation -- is it for security?" |
| **Duplications** | Blocks of code copy-pasted across files. SonarCloud reports a percentage of duplicated lines and the location of each duplicate. |
| **Cyclomatic complexity** | A measure of how many independent paths through a method. Methods with complexity >10 are flagged as hard to test and hard to reason about. |
| **Coverage** | Test coverage percentage, often integrated from a coverage report your test step produces. |

The point is not to drive every metric to zero. The point is to keep the trend honest: *new* code should not introduce new findings, and serious findings should not accumulate without a deliberate decision.

### SonarCloud vs SonarQube

The same engine, different hosting:

- **SonarCloud** is the SaaS offering: hosted by SonarSource, free for public repos, paid per lines-of-code for private. No infrastructure to run. Integrates natively with GitHub, GitLab, Bitbucket, and Azure DevOps.
- **SonarQube** is the self-hosted offering: you run a Java server (and a database) on your own infrastructure. Free Community Edition; paid Developer / Enterprise / Data Center editions. Required when corporate policy forbids sending source code to third-party SaaS.

For this course we use SonarCloud. The workflow steps and rule semantics are identical to SonarQube, so the skill transfers if you later land in a self-hosted environment.

### Setting Up SonarCloud Against a GitHub Repo

The setup has three sides: the SonarCloud account, the GitHub App, and the project configuration in your repo.

1. **Create a SonarCloud account** at sonarcloud.io, signing in with your GitHub account.
2. **Create or select a SonarCloud organization.** Organizations group projects. The simplest path is to import a GitHub organization 1:1 -- SonarCloud will create a matching organization, and selecting "Auto Configuration" applies the default analysis settings to projects in that org.
3. **Install the SonarCloud GitHub App** on the GitHub organization (or on the specific repos). The App is what posts PR decoration comments and reports the quality-gate status check. Without it, SonarCloud can analyze the code but cannot annotate PRs.
4. **Add a project in SonarCloud** corresponding to your GitHub repo. Each project is identified by a **project key** (a unique string, usually `<org>_<repo>`) and an **organization key**. You will reference both from your workflow.
5. **Generate a SonarCloud token** under your account's Security settings. This is what your CI workflow uses to authenticate. Save it in the GitHub repo as a secret named `SONAR_TOKEN`.

Once configured, SonarCloud automatically picks up branch and PR analyses pushed by your CI workflow. PRs get an inline summary, individual file annotations, and a "Quality Gate" status check.

### Quality Gates

A **quality gate** is a set of pass/fail conditions evaluated against an analysis. If every condition passes, the gate is green; if any condition fails, the gate is red, and the corresponding GitHub status check fails -- which (combined with branch protection) blocks the PR from merging.

SonarCloud's default gate is called **Sonar Way**. Its conditions, at the time of writing, evaluate against *new code* -- code added or modified in the analyzed branch / PR relative to the main branch baseline:

| Condition | Threshold |
|---|---|
| Coverage on new code | >= 80% |
| Duplicated lines on new code | <= 3% |
| Maintainability rating on new code | A (best) |
| Reliability rating on new code | A |
| Security rating on new code | A |
| Security hotspots reviewed on new code | 100% |

The "new code" focus is deliberate. Holding existing code to a high bar is often unrealistic on a legacy codebase -- you would never get a green build. Holding *new* code to a high bar is achievable: every PR is small, the new code is right in front of the author, fixing it is cheap. Over time, as files are touched, the legacy ratings improve naturally.

You can also define a custom quality gate (for instance, requiring 90% coverage instead of 80%) and apply it per project.

### PR Decoration

When SonarCloud analyzes a PR, it does two things:

- **Posts a summary comment** to the PR with the gate result, counts of new bugs / vulnerabilities / code smells / hotspots, and the new-code coverage and duplication numbers.
- **Posts inline comments** on each line of the diff that triggered a finding, with the rule name and a fix suggestion link.

The reviewer sees the findings in the same place they review the code, without leaving GitHub. This is the change that takes static analysis from "a dashboard nobody looks at" to "a tool that catches bugs in code review."

### Adding SonarCloud to a GitHub Actions Workflow

For a JavaScript / TypeScript project (your React app), the official action is `SonarSource/sonarqube-scan-action`. The minimal addition to the CI workflow you wrote yesterday:

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    name: Build and Analyze
    runs-on: ubuntu-latest

    steps:
      - name: Check out source
        uses: actions/checkout@v4
        with:
          fetch-depth: 0   # SonarCloud needs full history for accurate blame.

      - name: Set up Node.js 20
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'

      - name: Install dependencies
        run: npm ci

      - name: Run tests with coverage
        run: npm test -- --coverage --watchAll=false

      - name: Build
        run: npm run build

      - name: SonarCloud Scan
        uses: SonarSource/sonarqube-scan-action@v4
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
          SONAR_HOST_URL: https://sonarcloud.io
        with:
          args: >
            -Dsonar.organization=my-github-org
            -Dsonar.projectKey=my-github-org_my-react-app
            -Dsonar.sources=src
            -Dsonar.javascript.lcov.reportPaths=coverage/lcov.info
```

A few notes:

- `fetch-depth: 0` tells `actions/checkout` to fetch the full Git history. SonarCloud uses Git blame for new-code detection; a shallow clone breaks it.
- `SONAR_TOKEN` comes from the secret you configured. Never commit it to the repo.
- `SONAR_HOST_URL` distinguishes SonarCloud from SonarQube. Always `https://sonarcloud.io` for SonarCloud.
- The coverage report path (`coverage/lcov.info`) is whatever your test runner produces. Jest with the `--coverage` flag writes to `coverage/lcov.info` by default; Vitest uses the same path with the appropriate config.

For a .NET project, you would use the `dotnet-sonarscanner` global tool with `SonarScanner` `begin` / `end` calls bracketing your build and test steps. The conceptual flow is identical.

### What Happens on a PR After This

1. Developer pushes a feature branch and opens a PR.
2. The CI workflow fires. It builds, tests with coverage, then runs the SonarCloud scan.
3. SonarCloud analyzes the diff against `main`, computes new-code metrics, evaluates the quality gate.
4. SonarCloud's GitHub App posts a summary comment and inline annotations to the PR.
5. SonarCloud reports the gate result as a status check (green or red) on the PR.
6. With branch protection requiring the SonarCloud check to pass, the PR cannot merge if the gate fails.
7. The developer either fixes the findings (preferred) or, for hotspots, reviews them in SonarCloud and marks them as Safe with a justification.

That is the full closed loop: static analysis is no longer advisory and no longer requires human discipline. It is a wall.

## Code Example: Reading a SonarCloud Finding

A typical inline annotation on a PR diff:

```text
Issue: Define a constant instead of duplicating this literal "Bearer " 5 times.
Severity: Minor   Rule: typescript:S1192   Type: Code Smell
File: src/api/client.ts, line 42
```

How to read it: the literal `"Bearer "` appears in five places in the codebase, which is a maintainability risk (rename it once and you must rename it everywhere). The fix is to extract it into a constant -- `const AUTH_PREFIX = "Bearer ";` -- and reference the constant.

A higher-severity example:

```text
Issue: Make sure that hardcoding this credential is safe here.
Severity: Blocker   Rule: typescript:S2068   Type: Vulnerability
File: src/config/dev.ts, line 17
```

The fix is to move the value into an environment variable read at runtime (or, in CI, into a GitHub secret) and never to commit the literal.

## Summary

- Static analysis examines source code without executing it; it complements dynamic analysis, which examines the running program.
- SonarCloud reports findings in categories: bugs, code smells, security vulnerabilities, security hotspots, duplications, complexity, coverage.
- SonarCloud is the SaaS variant of SonarQube; choose SonarCloud unless self-hosting is required.
- Setup is: SonarCloud organization + GitHub App install + project key + a `SONAR_TOKEN` secret in the repo.
- Quality gates evaluate against *new code* (code added in the analyzed branch/PR), which is what makes them realistic on a legacy codebase.
- The `SonarSource/sonarqube-scan-action` plus a `fetch-depth: 0` checkout is the minimum to run analysis from GitHub Actions.
- Pair the SonarCloud status check with branch protection on `main` to make the gate enforced rather than advisory.

## Additional Resources

- [SonarCloud Docs: Get started with GitHub](https://docs.sonarsource.com/sonarcloud/getting-started/github/)
- [SonarCloud Docs: Quality Gates](https://docs.sonarsource.com/sonarcloud/improving/quality-gates/)
- [GitHub Marketplace: SonarQube Cloud (SonarCloud) Scan action](https://github.com/marketplace/actions/sonarqube-scan)
