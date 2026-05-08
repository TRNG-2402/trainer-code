# Images and Containers

## Learning Objectives
- Define a Docker image as an immutable, layered template identified by name:tag and content digest.
- Define a Docker container as a running instance of an image with its own writable layer and namespaces.
- Map the one-image-to-many-containers relationship and why it matters.
- Walk the container lifecycle: created, running, paused, stopped, removed.
- Pick an appropriate base image (alpine, ubuntu, distroless, language-specific).

## Why This Matters

The single most common confusion for new Docker users is treating images and containers as interchangeable terms. They are not. Images are the build artifact; containers are the running thing. Almost every Docker command operates on one or the other, and naming them correctly in your head makes every subsequent command obvious.

## The Concept

### Docker Image

An **image** is an immutable, read-only template. Think of it as a packaged snapshot: filesystem layers + metadata (env vars, exposed ports, default command, working directory, user). Images are:

- **Built in layers.** Each Dockerfile instruction produces one layer. Layers are cached and reused across images.
- **Identified by name and tag.** `nginx:1.25-alpine` is the name `nginx` at tag `1.25-alpine`. If you skip the tag, Docker assumes `latest`.
- **Identified by digest.** A content-addressed `sha256:...` hash uniquely identifies image bytes. Tags can move to point at new bytes; digests cannot. Production deploys often pin to digests for that reason.
- **Stored in registries.** Docker Hub is the default. Private registries (ACR, ECR, GHCR) work the same way.

Critically, an image is **immutable**. You cannot edit an image. To "change" an image, you build a new one with a new tag.

### Docker Container

A **container** is a running (or stopped) instance of an image. When the daemon creates a container, it:

1. Stacks the image's read-only layers using the union filesystem.
2. Adds a thin **writable layer** on top -- this is where the container's runtime changes (new files, modified files, deleted files) live via copy-on-write.
3. Allocates Linux **namespaces**: PID (its own process tree), network (its own interfaces and IP), mount (its own filesystem view), UTS (its own hostname), IPC, user.
4. Applies **cgroups** to cap CPU, memory, and I/O.
5. Starts the image's default command (or one you supplied).

When the container stops, the writable layer persists (so `docker start` resumes it). When you `docker rm` it, the writable layer is destroyed -- gone for good.

### One Image, Many Containers

This is the relationship to internalize:

```
                +----------------+
                |     Image      |   nginx:1.25-alpine
                | (read-only)    |
                +----------------+
                       |
       +---------------+---------------+
       |               |               |
       v               v               v
+-------------+  +-------------+  +-------------+
| Container A |  | Container B |  | Container C |
| writable    |  | writable    |  | writable    |
| layer + ns  |  | layer + ns  |  | layer + ns  |
+-------------+  +-------------+  +-------------+
   port 8080       port 8081       port 8082
```

You can launch as many containers from one image as the host can hold. Each gets its own writable layer, its own filesystem view, its own network namespace. None of them can see the others' processes or files. The shared image bytes are read-only and consume disk only once.

This is also why containers are cheap: starting a new one is "allocate a writable layer + namespaces + start the process" -- not "boot an OS."

### Container Lifecycle

A container moves through a small set of states:

- **Created** -- the container exists (writable layer + namespaces allocated) but has not started. `docker create ...` produces this state.
- **Running** -- the main process is executing. `docker start` or `docker run` puts the container here.
- **Paused** -- all processes inside the container are frozen via cgroups freezer. `docker pause`.
- **Stopped (Exited)** -- the main process has exited (clean or via signal). `docker stop` sends SIGTERM, waits, then SIGKILL. The writable layer is preserved.
- **Removed** -- the container record and writable layer are deleted. `docker rm`.

`docker ps` shows running containers. `docker ps -a` shows all containers regardless of state -- useful for finding the exited one whose logs you need.

A subtle point: `docker run` is `docker create` + `docker start` rolled into one. When you debug create-vs-start failures, knowing they are separate steps helps.

### Choosing a Base Image

The first line of every Dockerfile is `FROM <base>`. The base sets the size, surface area, and behavior of everything you build on top.

Common bases:

- **`alpine`** (5-8 MB). Minimal Linux based on musl libc and busybox. Tiny, security-friendly. Trade-off: musl can break libraries that assume glibc; debugging tools are absent unless you install them.
- **`ubuntu` / `debian`** (30-80 MB). Familiar full distro with apt. Larger, but you can install anything quickly. Default for "I want it to just work."
- **`distroless`** (Google) -- only your app and its runtime, no shell, no package manager. Smallest possible attack surface. Hard to debug because you cannot exec a shell. Production-favorable.
- **Language-specific** -- `node:20-alpine`, `python:3.12-slim`, `mcr.microsoft.com/dotnet/aspnet:8.0`, `mcr.microsoft.com/dotnet/sdk:8.0`. Maintained by the language's vendor; the right starting point for that runtime.
- **`scratch`** -- empty. You add only what you need. Used for static binaries (Go, Rust). Smallest possible image.

For this week's .NET work, you will use:

- `mcr.microsoft.com/dotnet/sdk:8.0` for **build** stages (has the .NET SDK, restore + build + publish).
- `mcr.microsoft.com/dotnet/aspnet:8.0` for **runtime** stages of ASP.NET Core apps (ASP.NET runtime only, no SDK).
- `mcr.microsoft.com/dotnet/runtime:8.0` for **runtime** stages of console apps.

Picking the right runtime image is how you trim a 1 GB image down to 200 MB.

## Code Example

Walk an image and a container from creation to teardown:

```bash
# 1. Pull an image -- now it lives in the local image store.
docker pull nginx:1.25-alpine

# 2. List images. Note the size, tag, and image ID.
docker images

# 3. Inspect the image -- env vars, exposed ports, default cmd, layers.
docker inspect nginx:1.25-alpine

# 4. Create a container without starting it.
docker create --name web nginx:1.25-alpine

# 5. List all containers (including the just-created one).
docker ps -a

# 6. Start it.
docker start web

# 7. List running containers.
docker ps

# 8. Pause / unpause.
docker pause web
docker unpause web

# 9. Stop it (SIGTERM, then SIGKILL after timeout).
docker stop web

# 10. Remove the container -- writable layer destroyed.
docker rm web

# 11. Remove the image -- layers freed if no other image references them.
docker rmi nginx:1.25-alpine
```

Run that sequence once and the lifecycle becomes second nature.

## Summary

- An image is an immutable, layered, read-only template; a container is a running instance with its own writable layer and namespaces.
- One image yields many independent containers, each isolated by Linux namespaces and cgroups.
- Container states: created -> running -> paused -> stopped -> removed.
- Base image choice (alpine, distroless, ubuntu, language-specific) drives image size, attack surface, and debuggability.
- For .NET, use `dotnet/sdk` for build, `dotnet/aspnet` or `dotnet/runtime` for runtime.

## Additional Resources

- [Docker images vs containers (official)](https://docs.docker.com/get-started/docker-concepts/the-basics/what-is-an-image/)
- [Container lifecycle reference](https://docs.docker.com/engine/reference/commandline/container/)
- [Microsoft .NET base images on MCR](https://mcr.microsoft.com/en-us/catalog?cat=.NET&search=dotnet)
