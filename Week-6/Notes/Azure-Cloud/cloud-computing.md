# Cloud Computing Fundamentals

## Learning Objectives
- Define cloud computing using the NIST five essential characteristics.
- Distinguish the four deployment models (public, private, hybrid, community) and the trade-offs of each.
- Distinguish the three core service models (IaaS, PaaS, SaaS) and place common Azure offerings into the correct layer.
- Explain the shared responsibility model and what changes between IaaS, PaaS, and SaaS.

## Why This Matters

Every project from this point forward in the program lives in the cloud. The pipeline you will build on Thursday runs on a cloud-hosted runner, deploys to a cloud-hosted environment, and reports its results to a cloud-hosted dashboard. Without a clear vocabulary for *what kind of cloud service* you are using, you cannot reason about cost, security boundaries, or who is responsible when something breaks at 3am. The difference between "the database went down because Azure had an outage" and "the database went down because we forgot to patch the VM" is the shared responsibility model -- and that model only makes sense once you know whether you are on IaaS, PaaS, or SaaS.

This connects directly to the Week 4 epic: by Friday you will deploy a React app through a CI/CD pipeline. The pipeline runners, the artifact storage, the deployment target, and the quality gate dashboard are *all* cloud services -- and they are not all the same kind of cloud service.

## The Concept

### Definition (NIST)

The U.S. National Institute of Standards and Technology (NIST) gives the most widely cited definition of cloud computing. A service is "cloud" when it has all five of these characteristics:

1. **On-demand self-service.** A user can provision compute, storage, and network resources without human interaction with the provider. You click a button or run a CLI command -- you do not file a ticket and wait for an admin.
2. **Broad network access.** Resources are available over standard network protocols (typically HTTPS) and reachable from a wide range of clients (browser, CLI, mobile).
3. **Resource pooling.** The provider's hardware is multi-tenant. Many customers share the same physical machines, separated by virtualization. You do not know -- and do not need to know -- exactly which physical box your VM is running on.
4. **Rapid elasticity.** Capacity can scale up and down quickly, often automatically. From the consumer's perspective, capacity appears unlimited.
5. **Measured service.** Usage is metered (CPU-hours, GB-stored, requests-served). You pay for what you use.

If a service is missing one of these, it is not cloud computing. A datacenter where you have to email the ops team to provision a VM fails #1. A service that bills a flat monthly fee regardless of usage fails #5.

### Deployment Models

The deployment model answers *who can use this cloud and where does it live*.

| Model | Who uses it | Where it runs | Trade-off |
|---|---|---|---|
| **Public cloud** | Anyone who pays | Provider's datacenters, shared with all other customers | Cheapest, fastest to start, least control over the underlying hardware. Examples: Azure public regions, AWS, GCP. |
| **Private cloud** | One organization only | Either on-premises or hosted by a provider but dedicated hardware | Maximum control and isolation, often required for regulated industries (defense, healthcare in some jurisdictions). Higher cost, slower to scale. |
| **Hybrid cloud** | Mix: some workloads on public, some on private, with connectivity between them | Both | Lets you keep sensitive workloads on private cloud while bursting non-sensitive workloads to public for elasticity. More complex networking and identity. |
| **Community cloud** | A specific group of organizations with shared concerns (e.g., a consortium of banks, government agencies of one country) | Provider or shared facility | Costs are split across the community. Compliance posture is tailored to the community's needs. Rare in commercial work. |

### Service Models

The service model answers *how much of the stack the provider manages and how much you manage*.

Think of the stack from bottom to top: physical hardware -> virtualization -> operating system -> runtime -> application -> data.

- **IaaS (Infrastructure as a Service).** The provider manages physical hardware, virtualization, and the network. You manage everything from the OS up: patching, runtime install, application deployment, data. **Azure example: Virtual Machines.** You get a Windows or Linux box. You install .NET, you install the web server, you copy your code, you patch the OS every month.
- **PaaS (Platform as a Service).** The provider manages everything up through the runtime. You bring application code and configure the runtime. You do not patch the OS, you do not install .NET. **Azure example: App Service.** You upload your ASP.NET Core app or your React bundle, choose a runtime version, and the platform handles hosting, scaling, and OS patching.
- **SaaS (Software as a Service).** The provider manages the full stack including the application itself. You bring data and configuration. **Azure / Microsoft example: Office 365, Outlook on the web.** You do not deploy Outlook; you use it.

A useful mnemonic: **as you move from IaaS toward SaaS, you give up control in exchange for not having to do operational work.** A startup with no ops team should default to SaaS-then-PaaS. An organization with strict compliance constraints often has to take on more IaaS work because the higher layers do not meet their requirements.

### Shared Responsibility Model

The shared responsibility model is the contract between you and the cloud provider that says *who is responsible for which security and operational concerns*. It changes by service model.

A simplified view:

| Concern | IaaS | PaaS | SaaS |
|---|---|---|---|
| Physical security of datacenter | Provider | Provider | Provider |
| Network infrastructure | Provider | Provider | Provider |
| Hypervisor / virtualization | Provider | Provider | Provider |
| Operating system patches | **You** | Provider | Provider |
| Runtime (e.g., .NET, Node) patches | **You** | Provider | Provider |
| Application code | **You** | **You** | Provider |
| Application configuration | **You** | **You** | **You** |
| Identity and access management | **You** | **You** | **You** |
| **Your data** | **You** | **You** | **You** |

Two things are true at every layer: **you always own your data, and you always own who has access to it.** The provider will never recover data you deleted in error and will never decide who is allowed to log in to your app -- those are always your responsibility. Confusion about who patches the OS or who maintains the runtime is the most common source of preventable incidents.

## Code Example (Conceptual)

Cloud is a topic without runnable code at this stage. The most useful "code" is a classification exercise -- given a service, place it on the IaaS/PaaS/SaaS spectrum.

```text
Service                                    Model    Why
-----------------------------------------  -------  -------------------------------------------
Azure Virtual Machine running Windows      IaaS     You manage OS and everything above.
Azure App Service hosting ASP.NET Core     PaaS     Runtime and OS are managed.
Azure SQL Database                         PaaS     Database engine is managed; you own schema and data.
Azure Functions                            PaaS     Often called "serverless" but is a PaaS variant.
Microsoft 365 (Outlook, Teams)             SaaS     Application itself is provided.
GitHub.com                                 SaaS     You consume; you do not run the platform.
A self-hosted GitHub Enterprise server     IaaS-ish You run the binary on infrastructure you manage.
```

## Summary

- Cloud computing requires all five NIST characteristics. Missing any one (especially self-service or metered billing) means it is not cloud.
- Deployment models describe *who shares the hardware*: public (everyone), private (you alone), hybrid (mix), community (a defined group).
- Service models describe *how much of the stack the provider manages*: IaaS (least), PaaS (middle), SaaS (most).
- The shared responsibility model shifts with service model. You always own your data and access controls; everything else depends on which layer you are using.
- Choosing a service model is a trade-off between control and operational burden. Default to the highest-level service that meets your requirements.

## Additional Resources

- [NIST Definition of Cloud Computing (SP 800-145)](https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-145.pdf)
- [Microsoft Learn: What is cloud computing?](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/innovate/considerations/cloud-computing)
- [Microsoft Learn: Shared responsibility in the cloud](https://learn.microsoft.com/en-us/azure/security/fundamentals/shared-responsibility)
