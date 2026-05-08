# Docker Engine Architecture

## Learning Objectives
- Identify the components of the Docker Engine: the daemon, the REST API, and the CLI client.
- Explain the client-server model that connects `docker` commands to the daemon.
- Name the core Docker objects: images, containers, networks, volumes, registries.
- Trace what happens when you run `docker run` from the keystroke to a running container.
- Describe the layered, copy-on-write filesystem and why it makes images small and fast.
- Recognize `containerd` and `runc` as the lower-level pieces under the daemon.

## Why This Matters

When you type `docker run`, a lot happens. Knowing what each piece does -- and which piece is failing -- is the difference between "Docker is broken" and "the daemon is not running" or "the registry rejected my pull." This is the mental model you debug from for the rest of your career.

## The Concept

### Docker Engine Components

The **Docker Engine** is the umbrella name for three things that work together:

1. **`dockerd` -- the Docker daemon.** A long-running background service on the host. It owns the container lifecycle, builds images, manages networks and volumes, and talks to the registry. Nothing happens with containers unless `dockerd` is running.
2. **The REST API.** `dockerd` exposes an HTTP API (over a Unix socket on Linux, a named pipe on Windows). All container operations are HTTP calls under the hood.
3. **`docker` -- the CLI client.** The command-line tool you type into. It is a thin client that translates `docker run`, `docker ps`, etc. into REST calls against the daemon.

This is a **client-server architecture**. The CLI is just one possible client. Docker Desktop's UI is another client. Your CI script using a Docker SDK is a third. They all speak the same REST API to the same daemon.

```
+----------------+         +-----------------+         +------------------+
|  docker CLI    | ------> |  Docker daemon  | ------> |  containerd /    |
|  (or Desktop)  |  REST   |  (dockerd)      |         |  runc            |
+----------------+         +-----------------+         +------------------+
                                    |
                                    v
                          +-----------------+
                          |   Registry      |
                          |   (Docker Hub)  |
                          +-----------------+
```

### Docker Objects

The daemon manages a small set of object types. Almost every Docker command operates on one of these:

- **Images** -- read-only templates. Built in layers. Identified by `name:tag` (e.g., `nginx:1.25-alpine`) and an immutable content digest (`sha256:...`).
- **Containers** -- running (or stopped) instances of an image. Each has its own filesystem layer, processes, and network namespace.
- **Networks** -- virtual networks that connect containers (default `bridge`, `host`, `none`, plus user-defined).
- **Volumes** -- managed persistent storage for containers, kept outside the container's writable layer.
- **Registries** -- remote services that store and distribute images (Docker Hub, Azure Container Registry, AWS ECR, GitHub Container Registry).

You will use `docker image`, `docker container`, `docker network`, `docker volume` subcommands to manage each.

### What Happens When You Run `docker run nginx`

This is the canonical flow. Walk through it step by step:

1. **CLI parses the command.** `docker` reads `run` and the arguments, builds an HTTP request.
2. **Request hits the daemon's REST API.** The CLI connects to the daemon's socket and sends the request.
3. **Image lookup.** The daemon checks its local image store for `nginx:latest`. If present, skip to step 5.
4. **Image pull (if needed).** The daemon contacts the registry (Docker Hub by default), authenticates if required, downloads the image manifest, then downloads each missing layer in parallel. Layers are content-addressed, so layers shared with other images are not re-downloaded.
5. **Container create.** The daemon prepares a writable layer on top of the image, allocates a network namespace and IP, sets up volumes, and registers the container.
6. **Container start.** The daemon hands off to the lower-level runtime (`containerd`, then `runc`) which uses Linux namespaces and cgroups to launch the container's main process.
7. **Container running.** Logs stream back to the daemon; the CLI either detaches (`-d`) or attaches to the container's stdout/stderr.

If anything in this chain fails, the error message tells you which step: image-not-found errors come from step 3-4; permission errors usually come from step 1-2; "OCI runtime create failed" comes from step 6.

### Layered Filesystem (Union FS, Copy-on-Write)

A Docker image is **a stack of layers**, not a single blob. Each instruction in a Dockerfile (`FROM`, `RUN`, `COPY`, etc.) produces one layer. The container engine uses a **union filesystem** (overlay2 on modern Linux) to stack layers on top of each other, presenting a single merged filesystem to the container.

Two consequences fall out of this design:

1. **Layer reuse.** If two images both start `FROM ubuntu:22.04`, they share the Ubuntu layers on disk. Pulling a second image based on Ubuntu only downloads the layers above the shared base. This is why image storage is far smaller than the sum of image sizes.

2. **Copy-on-write.** When a running container writes to a file that exists in a lower (image) layer, the file is copied up into the container's writable layer first, then modified. The image itself is never changed. Stop the container, start a new one from the same image, and the writable layer is fresh.

That is why containers are cheap to throw away: their state lives in the writable layer, and the immutable image is reusable forever.

### What Lives Beneath the Daemon: containerd and runc

`dockerd` does not actually start your container's process itself. It delegates to lower-level components:

- **`containerd`** -- a separate daemon that manages the full container lifecycle on the host: image pulls, snapshots (the layered FS), and supervising running containers. `dockerd` calls `containerd`. Kubernetes also calls `containerd` directly without Docker in the picture.
- **`runc`** -- the OCI-compliant runtime that actually creates the container process by configuring namespaces and cgroups, then `exec`s the application. `containerd` invokes `runc` once per container.

You will rarely interact with these directly, but knowing they exist explains why "Docker" and "Kubernetes" can coexist on the same host without conflict: Kubernetes uses `containerd` directly and skips `dockerd`.

## Code Example

Inspect the architecture from the CLI:

```bash
# Show daemon version, OS/arch, storage driver, runtime info.
docker info

# Show client and server (daemon) versions side by side.
docker version

# Show layer history of an image -- one entry per Dockerfile instruction.
docker history nginx:latest

# Inspect a running container -- network, mounts, layered FS, runtime args.
docker inspect <container_id>
```

`docker info` is your "is the daemon healthy?" probe. `docker version` distinguishes "the CLI is fine but the daemon is down" from "everything is up."

## Summary

- Docker Engine = `dockerd` daemon + REST API + `docker` CLI; the CLI is just one client of the daemon.
- The daemon manages images, containers, networks, volumes, and talks to registries.
- `docker run` flows: CLI -> REST -> image pull (if needed) -> container create -> handoff to `containerd`/`runc` -> running process.
- Images are stacks of layers; a union filesystem (overlay2) merges them, and copy-on-write isolates container writes from the immutable image.
- Beneath `dockerd`, `containerd` manages container lifecycles and `runc` actually starts the process.

## Additional Resources

- [Docker architecture (official docs)](https://docs.docker.com/get-started/overview/#docker-architecture)
- [containerd project](https://containerd.io/)
- [runc on GitHub](https://github.com/opencontainers/runc)
