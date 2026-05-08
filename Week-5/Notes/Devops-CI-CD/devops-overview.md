# DevOps Overview

## Learning Objectives
- Define DevOps as the combination of culture, practices, and tools (not a job title or a single tool).
- Walk through the eight phases of the DevOps lifecycle and describe what happens in each.
- Explain how DevOps and Agile complement each other rather than compete.
- Recognize the core DevOps principles that drive specific tooling and process choices.
- Recognize the common DevOps tool categories and where they fit in the lifecycle.

## Why This Matters

You have spent the last three weeks writing code: EF Core models, ASP.NET Core APIs, React components. None of that work creates value until it reaches a user. DevOps is the discipline that closes the gap between "code merged" and "user benefits." Every minute that gap takes is a minute the business is paying for software it cannot use.

By Friday of this week you will run a real CI/CD pipeline that takes a commit, builds it, tests it, scans it for quality issues, and produces a deployable artifact -- in under a few minutes, with no human intervention. That pipeline is the most visible artifact of DevOps, but the pipeline is the easy part. The hard part is the cultural shift the pipeline depends on: developers owning quality, operations owning automation, both teams sharing responsibility for the running system. This reading sets up the vocabulary and principles you need before we touch the tools.

## The Concept

### What DevOps Is (and Is Not)

A common definition: DevOps is **culture + practices + tools** that shorten the system development life cycle and provide continuous delivery with high software quality.

Read that as three layers:

- **Culture.** Developers and operations work as one team with shared goals. There is no "throw it over the wall" handoff. Failure is treated as a learning opportunity, not a blame event. Quality is everyone's responsibility, not just QA's.
- **Practices.** Concrete behaviors: code review on every change, automated tests run on every commit, infrastructure defined as code, deployments rolled out in small batches, production monitored continuously, post-incident reviews conducted blamelessly.
- **Tools.** The software that enables the practices: version control, CI/CD platforms, configuration management, monitoring, log aggregation, incident response.

Common misunderstandings to discard now:

- DevOps is **not a job title.** A "DevOps Engineer" usually does platform engineering or release engineering work, but having one person with that title does not mean an organization "does DevOps."
- DevOps is **not a single tool.** Buying GitHub Actions or Azure DevOps does not make a team a DevOps team any more than buying a piano makes you a pianist.
- DevOps is **not the same as automation.** Automation is one practice within DevOps. A team that automates deployment but does not change its culture or its quality practices is not doing DevOps; it is doing scripted ops.

### The DevOps Lifecycle

The DevOps lifecycle is a continuous loop, often drawn as an infinity symbol with eight phases. The loop never ends -- the output of "Monitor" feeds back into the next "Plan."

| # | Phase | What happens |
|---|---|---|
| 1 | **Plan** | Backlog refinement, sprint planning, work item creation. Where business intent becomes engineering work. |
| 2 | **Code** | Developers write code in feature branches, review each other's pull requests, merge to a shared trunk. |
| 3 | **Build** | The CI server compiles the code, resolves dependencies, and produces an artifact (a binary, a container image, a static bundle). |
| 4 | **Test** | Automated tests run against the artifact: unit, integration, sometimes end-to-end and security scans. |
| 5 | **Release** | The tested artifact is promoted to a release candidate, versioned, and stored in an artifact registry. |
| 6 | **Deploy** | The artifact is rolled out to an environment (dev, staging, production) using an automated process. |
| 7 | **Operate** | The application runs and serves users. The platform team handles capacity, scaling, on-call response. |
| 8 | **Monitor** | Logs, metrics, and traces are collected. Alerts fire on degradation. Findings feed back into the next planning cycle. |

The first four phases are sometimes called "Dev" and the last four "Ops." DevOps is the deliberate erasure of that boundary -- developers care about Operate and Monitor, operations engineers care about Plan and Code.

### DevOps and Agile

DevOps and Agile are often confused or conflated. They are not the same thing, and they are not in competition. They operate at different scopes:

- **Agile** is about *delivery cadence and team responsiveness*: short iterations, working software over comprehensive documentation, customer collaboration, response to change. Agile says: *"Plan in two-week chunks and ship working software at the end of each chunk."*
- **DevOps** is about *deployment velocity and operational quality*: how the working software at the end of an Agile sprint actually gets to users, and how the team learns from it once it is there. DevOps says: *"And while you are at it, ship every commit through automation, monitor what you ship, and learn from production."*

You can be Agile without DevOps -- a team that does two-week sprints but ships to production once a quarter through a manual change-control process is Agile but not DevOps. You can theoretically be DevOps without Agile -- a team with a fully automated pipeline working from a multi-year waterfall plan -- but in practice the two reinforce each other so strongly that they are almost always paired.

The clean way to remember it: **Agile is how you organize the work; DevOps is how you deliver and operate the work.**

### Core Principles

A handful of principles drive most DevOps tooling and process choices. When you are deciding whether a practice is "DevOps-aligned," check it against these:

1. **Automation.** If a step is repeated and rule-based, automate it. Manual steps are slow, inconsistent, and not auditable. Pipelines, infrastructure-as-code, automated tests all flow from this.
2. **Collaboration.** Tear down silos between development, QA, security, and operations. Shared on-call, shared metrics, shared retrospectives.
3. **Continuous feedback.** The team should learn quickly when something is wrong: fast tests in CI, fast alerts in production, fast post-incident reviews. The opposite is "discover the regression three months later."
4. **Fail fast (and fail safely).** It is better to break a deployment in the first 30 seconds with a clear error than to break it 20 minutes in with a half-applied state. Design pipelines and rollouts so failures are loud, contained, and reversible.
5. **Small batch sizes.** Ship many small changes rather than few large ones. Small changes are easier to review, easier to test, easier to roll back, and easier to reason about when something goes wrong.
6. **Measure everything.** Cycle time, deployment frequency, change failure rate, mean time to recovery (these four are the classic "DORA metrics"). What you do not measure, you cannot improve.

### Tool Landscape

You do not need to memorize every tool, but you should recognize the categories. Most stacks have one tool from each:

| Category | Examples |
|---|---|
| **Source control** | Git, hosted on GitHub, GitLab, Bitbucket, or Azure Repos. |
| **CI/CD** | GitHub Actions, Azure Pipelines, GitLab CI, Jenkins, CircleCI. |
| **Artifact registry** | GitHub Packages, Azure Artifacts, Docker Hub, JFrog Artifactory. |
| **Configuration management** | Ansible, Chef, Puppet. |
| **Infrastructure as code** | Terraform, Bicep, AWS CloudFormation, Pulumi. |
| **Containerization & orchestration** | Docker (covered Week 5), Kubernetes. |
| **Monitoring & observability** | Azure Monitor, Datadog, New Relic, Grafana, Prometheus. |
| **Log aggregation** | Splunk, Elastic Stack (ELK), Azure Log Analytics. |
| **Code quality / security** | SonarCloud (covered Friday), Snyk, GitHub CodeQL. |

Tomorrow you will see GitHub-the-platform fill the source control, work tracking, CI/CD, and artifact-registry categories all at once.

## Code Example (Conceptual)

DevOps is a discipline rather than a snippet, but you can sanity-check whether a given practice is DevOps-aligned by mapping it to the lifecycle and principles. Two examples:

```text
Practice: "Run unit tests on every PR; block merge if any test fails."
Lifecycle phase: Test (in CI)
Principles satisfied: Automation, Continuous feedback, Fail fast
Verdict: DevOps-aligned.

Practice: "Once a quarter, the ops team manually copies the latest build
to a production server during a 4-hour maintenance window."
Lifecycle phase: Deploy (manual)
Principles violated: Automation, Small batch sizes, Continuous feedback
Verdict: Not DevOps-aligned. Risk and lead time are both high.
```

## Summary

- DevOps is culture + practices + tools, not a job title and not a single tool.
- The eight-phase lifecycle (Plan, Code, Build, Test, Release, Deploy, Operate, Monitor) is a continuous loop with no clear "developer" or "operations" boundary.
- Agile sets delivery cadence; DevOps delivers and operates the work. They are complementary.
- Six core principles -- automation, collaboration, continuous feedback, fail fast, small batches, measure everything -- justify most DevOps tool and process choices.
- The tool landscape is broad but slots into a small number of categories. You will meet most of these by the end of Week 5.

## Additional Resources

- [Atlassian: What is DevOps?](https://www.atlassian.com/devops)
- [Microsoft Learn: What is DevOps?](https://learn.microsoft.com/en-us/devops/what-is-devops)
- [Google Cloud: DORA research and the four key metrics](https://cloud.google.com/devops)
