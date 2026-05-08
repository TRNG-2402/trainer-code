# CI/CD: Continuous Integration, Delivery, and Deployment

## Learning Objectives
- Define Continuous Integration (CI), Continuous Delivery (CD), and Continuous Deployment and explain how they differ.
- Identify the typical stages of a CI/CD pipeline and what each stage proves.
- Explain the benefits CI/CD provides and the most common ways teams undermine those benefits.
- Recognize when Continuous Deployment is appropriate and when Continuous Delivery (with a manual gate) is the safer choice.

## Why This Matters

A pipeline is the most concrete artifact you will produce this week. It is also the most concrete embodiment of the DevOps principles you read about in the previous lesson. Tomorrow you will write a real pipeline file in YAML; on Friday you will add a quality gate to it. Before any of that, you need to be precise about what CI, Continuous Delivery, and Continuous Deployment each mean -- because the three terms are routinely used interchangeably in industry, and that imprecision causes real bugs (and real outages) when teams disagree on what the pipeline is actually supposed to do.

The Week 4 epic is to ship a React + ASP.NET Core app through an automated pipeline with quality enforcement. The acronym CI/CD covers the *how* of that goal.

## The Concept

### Continuous Integration (CI)

**Definition.** Every developer merges their work into a shared mainline frequently -- typically multiple times per day -- and every merge triggers an automated build and test run that verifies the change does not break the mainline.

The discipline has three parts:

1. **Merge frequently.** Long-lived feature branches accumulate divergence. Two weeks of separate work means two weeks of merge conflicts, two weeks where unrelated changes can subtly interact. Daily merges keep changes small and conflicts trivial.
2. **Build and test automatically on every commit.** A "CI server" (the platform running the build) listens for commits, checks out the code, compiles it, and runs the test suite. The result -- pass or fail -- is reported back to the commit.
3. **Fix breakage immediately.** A red main branch is a team-wide emergency. The team's first job in the morning is the same as their last job at night: keep main green.

The CI server's role is non-negotiable. Without it, "we should integrate frequently" is a wish; with it, integration is a fact verified on every push. GitHub Actions, Azure Pipelines, GitLab CI, and Jenkins are all CI servers in this sense.

The point of CI is the **fast feedback loop**: a developer who introduces a bug should hear about it within minutes, while the change is still fresh in their head, not three days later when context has decayed.

### Continuous Delivery

**Definition.** Every change that passes CI produces a release-ready artifact that *could* be deployed to production at any moment. The decision to actually deploy is a manual one -- typically a human clicks an approval button.

The shift from CI to Continuous Delivery is the shift from "we can build and test it" to "we can ship it." That requires:

- The pipeline produces a deployable artifact (a versioned binary, a container image, a static bundle), not just a build report.
- The pipeline can deploy that artifact to a staging environment automatically and run integration tests there.
- A human approval gate stands between staging and production. The gate is the *only* manual step.

Continuous Delivery is appropriate when production deployment carries enough risk -- regulatory, financial, reputational -- that a human should consciously sign off on each release. It is also the right level for teams that are early in their DevOps maturity: the pipeline does the work, but the human keeps the safety net.

### Continuous Deployment

**Definition.** Every change that passes CI is deployed to production automatically, with no manual approval. If the tests pass, the change ships.

Continuous Deployment is the same pipeline as Continuous Delivery with the manual gate removed. It is more demanding because the safety net moves from "a human's judgment" to "the test suite and the rollout strategy." Teams that practice Continuous Deployment typically rely on:

- Comprehensive automated test coverage (unit, integration, end-to-end, contract, sometimes load).
- Progressive rollouts: deploy to 1% of production traffic first, watch metrics, then 10%, then 100%. If a metric degrades, automatically roll back.
- Feature flags: ship code dark, then turn it on for users separately. If something breaks, flip the flag instead of redeploying.
- Strong observability: alerts fire within seconds when error rates or latency degrade.

Continuous Deployment is appropriate when the test suite genuinely catches real bugs and the cost of a single bad deploy is bounded by automated rollback. It is dangerous when the team is using it as a substitute for testing.

### CI vs CD vs CD: side-by-side

| Capability | CI | Continuous Delivery | Continuous Deployment |
|---|---|---|---|
| Auto-build on commit | Yes | Yes | Yes |
| Auto-run tests on commit | Yes | Yes | Yes |
| Produce deployable artifact | Optional | Yes | Yes |
| Auto-deploy to staging | No (typical) | Yes | Yes |
| Manual approval to production | N/A | Yes | No |
| Auto-deploy to production | No | No | Yes |

Notice that the three terms are stacked: Continuous Delivery includes CI; Continuous Deployment includes Continuous Delivery. You cannot do Continuous Deployment without first doing the work to do CI well.

### Pipeline Stages

A typical pipeline strings the work into stages, each gated on the success of the previous one. A representative pipeline for a React + ASP.NET Core project:

| Stage | Purpose | Typical actions |
|---|---|---|
| **Source / Trigger** | A push or PR fires the pipeline. | Webhook from the source control system; the pipeline checks out the commit. |
| **Build** | Compile the code, produce binaries and bundles. | `dotnet build` for the API, `npm run build` for the React app. |
| **Unit test** | Verify each unit of code in isolation. | `dotnet test`, `npm test`. Fast: typically under a few minutes. Failure stops the pipeline. |
| **Package / Artifact** | Bundle the build output as a versioned, immutable artifact. | Publish a `.zip` of the API, a `dist/` of the React app, a container image, or a NuGet/npm package. |
| **Integration test** | Verify components work together. | Start the API against a real (or in-memory) database; run tests that exercise HTTP endpoints. Slower than unit tests. |
| **Deploy to staging** | Roll the artifact out to a non-production environment that mirrors production. | Push to a staging slot; run smoke tests. |
| **Approval gate** | (Continuous Delivery only.) A human reviews and approves promotion. | Manual click in the CI/CD UI; sometimes a change-management ticket. |
| **Deploy to production** | Roll the artifact out to live users. | Blue-green or progressive rollout; automated health checks; auto-rollback on failure. |

Two important properties of a well-designed pipeline:

- **Fail fast.** Cheap, fast checks (lint, unit tests) run before expensive ones (integration tests, deployments). A linting failure should stop the pipeline within seconds, not after 20 minutes of integration tests have run.
- **Same artifact, every environment.** The artifact built in the Build stage is the *same one* deployed to staging *and* to production. Rebuilding for production is a recipe for "works on staging, broken in production" because the build inputs may differ.

### Benefits

- **Faster feedback.** Bugs are caught minutes after introduction, not days or weeks.
- **Smaller blast radius.** Small, frequent changes are easier to diagnose and roll back than large, infrequent ones.
- **Higher confidence.** A green pipeline run is a strong signal that a change is safe.
- **Lower deployment cost.** Deployments become a routine event rather than a quarterly drama.
- **Auditability.** Every deployment to production is tied to a specific commit, with a recorded test result and (for Continuous Delivery) a recorded human approver.

### Common Pitfalls

- **Slow pipelines.** When the pipeline takes 45 minutes, developers stop running it locally and start batching their work. The fast feedback loop dies.
- **Flaky tests.** A test that fails 1 in 20 runs for unrelated reasons trains the team to ignore failures. Once developers retry-until-green, the pipeline stops being a signal.
- **Manual steps hidden in "automated" pipelines.** A pipeline with a step that says "now SSH into the server and run the migration" is not a CI/CD pipeline. It is a partially scripted manual deployment.
- **No rollback path.** A pipeline that can deploy but cannot un-deploy is half a pipeline. Either keep the previous artifact warm (blue-green) or make migrations strictly backward-compatible.
- **Skipping environments.** "We will deploy directly to prod for this hotfix" defeats the purpose of staging. Hotfixes are when staging matters most.
- **Overgrown unit-test suite, undergrown integration-test suite.** A high count of unit tests with no integration tests means the pieces are individually correct but the system is unverified.

## Code Example (Conceptual Pipeline)

A YAML-shaped sketch (intentionally pseudocode -- you will write a real GitHub Actions workflow tomorrow):

```yaml
trigger:
  - push to main
  - pull request to main

stages:
  - name: Build
    jobs:
      - dotnet build src/Api
      - npm install && npm run build  # in src/Web

  - name: Unit Test
    jobs:
      - dotnet test src/Api.Tests
      - npm test --watchAll=false

  - name: Package
    jobs:
      - publish Api.zip with version 1.0.${BUILD_ID}
      - publish web-dist.zip

  - name: Integration Test
    jobs:
      - start API against in-memory database
      - run HTTP-level tests against it

  - name: Deploy to Staging
    jobs:
      - deploy Api.zip to staging environment
      - deploy web-dist.zip to staging static host
      - run smoke tests

  # Continuous Delivery: gate here.
  - name: Approval
    jobs:
      - require manual approval from a release engineer

  - name: Deploy to Production
    jobs:
      - deploy SAME Api.zip and web-dist.zip from the Package stage
      - run production health checks
      - auto-rollback if health checks fail
```

Read top to bottom: each stage is gated on the previous one's success, the artifact built once is reused everywhere downstream, and the only manual step (in the Continuous Delivery flavor) is the approval before production.

## Code Example: A Real Test Step

Here is what the unit-test stage actually looks like in a GitHub Actions workflow — the exact YAML you will be writing tomorrow. The job fails (and blocks the merge) if any test fails, so the pipeline stops before producing an artifact or deploying.

**ASP.NET Core API — `dotnet test`:**

```yaml
jobs:
  api-test:
    name: Build and test API
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore src/Api/Api.sln

      - name: Build
        run: dotnet build src/Api/Api.sln --configuration Release --no-restore

      - name: Test
        run: >
          dotnet test src/Api/Api.sln
          --configuration Release
          --no-build
          --verbosity normal
          --logger "trx;LogFileName=test-results.trx"
          --collect:"XPlat Code Coverage"

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: api-test-results
          path: '**/test-results.trx'
```

**React app — `npm test`:**

```yaml
jobs:
  web-test:
    name: Build and test web
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/Web
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/Web/package-lock.json

      - name: Install
        run: npm ci

      - name: Test
        # CI=true (default in Actions) tells most JS test runners to run once and exit
        # rather than entering watch mode.
        run: npm test -- --watchAll=false --coverage

      - name: Build
        run: npm run build
```

Three properties of these snippets that map back to the principles above:

- **Fail fast**: `restore` and `build` precede `test`, but every step is short and the test step fails the job at the first failed test.
- **No manual step**: nothing in the YAML waits for human input.
- **Same artifact contract**: the build output (`dotnet publish`/`npm run build` artifacts) is what downstream stages deploy — the test step proves it is safe to do so.

## Summary

- **CI** = merge frequently and verify automatically. The CI server enforces it.
- **Continuous Delivery** = always ship-ready, with a human approval before production.
- **Continuous Deployment** = no human gate; tests and rollout strategy carry the safety.
- A well-designed pipeline is fast, fails early, builds the artifact once, and uses the same artifact in every environment.
- Slow pipelines, flaky tests, and hidden manual steps are the most common failure modes -- they make the pipeline lie about quality.

## Additional Resources

- [Martin Fowler: Continuous Integration](https://martinfowler.com/articles/continuousIntegration.html)
- [Atlassian: Continuous integration vs delivery vs deployment](https://www.atlassian.com/continuous-delivery/principles/continuous-integration-vs-delivery-vs-deployment)
- [GitHub Docs: About continuous integration with GitHub Actions](https://docs.github.com/en/actions/automating-builds-and-tests/about-continuous-integration)
