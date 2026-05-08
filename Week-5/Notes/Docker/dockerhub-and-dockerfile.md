# Docker Hub and the Dockerfile

## Learning Objectives
- Use Docker Hub as a registry: log in, push, pull, name, and tag images.
- Distinguish official images (`library/nginx`) from user-namespaced images (`username/myapp`).
- Apply tagging conventions: `:latest`, semver, git SHA.
- Author a Dockerfile using `FROM`, `WORKDIR`, `COPY`, `ADD`, `RUN`, `ENV`, `EXPOSE`, `CMD`, `ENTRYPOINT`.
- Distinguish `CMD` vs `ENTRYPOINT` and pick the right one.
- Use a `.dockerignore` to keep build context small and clean.
- Write a multi-stage build to ship a small runtime image.
- Order Dockerfile instructions to maximize layer cache hits.

## Why This Matters

The Dockerfile is the source of truth for how your app is built and shipped. It is what your team commits, reviews, and deploys -- and it is what makes "works on my machine" go away forever. Docker Hub is where most images live, including the base images you build on. By the end of this topic, you can author and publish your own image.

## The Concept

### Docker Hub: The Default Registry

A **registry** is a service that stores images. Docker Hub (`hub.docker.com`) is the default registry the daemon talks to when you `docker pull` without specifying a host.

Images on Docker Hub fall into two namespaces:

- **Official images** -- maintained by Docker, Inc. and the project teams. They live in the `library/` namespace and you can refer to them by short name: `nginx` is shorthand for `library/nginx`. Examples: `nginx`, `redis`, `postgres`, `python`, `node`, `ubuntu`, `alpine`. Official images are vetted, regularly rebuilt, and the right starting point for most stacks.
- **User / organization images** -- everything else, namespaced by username or org: `myusername/myapp`, `microsoft/mssql-server`. Anyone with an account can publish.

Other registries work the same way but require a fully-qualified host: `mcr.microsoft.com/dotnet/aspnet:8.0`, `ghcr.io/owner/repo:tag`, `myregistry.azurecr.io/myapp:1.0`.

### Authenticating, Pushing, Pulling

```bash
# Sign in (creates ~/.docker/config.json with a token).
docker login

# Build an image with a name that includes your Hub username.
docker build -t myuser/productcatalog:1.0.0 .

# Push it to Docker Hub.
docker push myuser/productcatalog:1.0.0

# On another machine, pull it.
docker pull myuser/productcatalog:1.0.0

# Sign out (removes the token).
docker logout
```

If you forget the username prefix when tagging, `docker push` will fail because Docker tries to push to `library/` (the official namespace) and rejects you.

### Tagging Conventions

A tag is a label on an image -- typically a version. Three conventions are worth knowing:

- **`:latest`** -- the default tag if you omit one. Misleadingly named: `latest` is just the tag that nobody bothered to set, not always the newest. Avoid relying on it in production.
- **Semantic versioning** -- `1.0.0`, `1.0.1`, `1.1.0-beta`. The right choice for libraries and apps consumed by humans.
- **Git SHA** -- `myapp:a1b2c3d`. The right choice for CI/CD: every build gets a unique tag pinned to the commit. No "what code is in this tag?" ambiguity.

Many teams ship multiple tags per release: `myapp:1.4.2`, `myapp:1.4`, `myapp:1`, and `myapp:latest` all point at the same digest. Consumers pin to the precision they need.

### The Dockerfile: Instructions That Matter

A Dockerfile is a recipe of instructions. Each instruction produces a new layer. The set of instructions you actually use day to day is small.

**`FROM <image>:<tag>`** -- the base image. Always the first non-comment instruction. A Dockerfile can have multiple `FROM` lines for multi-stage builds.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```

**`WORKDIR /path`** -- set the working directory for subsequent `RUN`, `CMD`, `COPY`, `ADD`, `ENTRYPOINT`. Creates the directory if missing.

```dockerfile
WORKDIR /app
```

**`COPY <src> <dest>`** -- copy files from the build context into the image. Always prefer `COPY` over `ADD` unless you specifically need `ADD`'s extra features.

```dockerfile
COPY ./src ./src
COPY ProductCatalog.csproj .
```

**`ADD <src> <dest>`** -- like `COPY`, but also can fetch URLs and auto-extract local tar archives. Avoid unless you actually need those behaviors -- the implicit magic is a footgun.

**`RUN <command>`** -- execute a command at build time. Each `RUN` adds a layer.

```dockerfile
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
```

Combining commands with `&&` and cleaning up in the same `RUN` keeps the resulting layer small.

**`ENV KEY=VALUE`** -- set an environment variable that persists into the running container.

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
```

**`EXPOSE <port>`** -- documents that the container listens on a port. It does NOT publish the port; that is `-p` at run time. Treat `EXPOSE` as a hint to readers and tools.

```dockerfile
EXPOSE 8080
```

**`CMD ["executable", "arg1", "arg2"]`** -- the default command run when the container starts. Can be overridden by passing args to `docker run`.

**`ENTRYPOINT ["executable"]`** -- the command that always runs; arguments to `docker run` are passed as arguments to the entrypoint, not as a replacement.

### `CMD` vs `ENTRYPOINT`

This pair confuses everyone the first time. The rules:

- **Use `CMD` alone** when the container's command is fully overridable (`docker run myimage echo hello` should run `echo hello`).
- **Use `ENTRYPOINT` alone (or with `CMD` for default args)** when the container is essentially a wrapper around one fixed program and `docker run` arguments should be passed to it.

```dockerfile
# Pattern A: CMD alone -- fully overridable.
CMD ["dotnet", "ProductCatalog.dll"]
# docker run myapp                  -> dotnet ProductCatalog.dll
# docker run myapp ls /             -> ls /  (CMD replaced)

# Pattern B: ENTRYPOINT + CMD -- entrypoint fixed, CMD as default args.
ENTRYPOINT ["dotnet"]
CMD ["ProductCatalog.dll"]
# docker run myapp                  -> dotnet ProductCatalog.dll
# docker run myapp Other.dll        -> dotnet Other.dll (CMD replaced, entrypoint kept)
```

Always use the **exec form** (JSON array) for both. The shell form (`CMD echo hello`) wraps the command in `/bin/sh -c`, which breaks signal handling -- your container will not respond to `SIGTERM` properly and `docker stop` will fall back to `SIGKILL`.

### `.dockerignore`

The build context is everything in the directory you point `docker build` at. The daemon receives all of it before it can `COPY`. A bloated context means slow builds and risks copying secrets or huge files into your image.

A `.dockerignore` works exactly like `.gitignore` -- list patterns to exclude from the build context.

A solid baseline for a .NET + Node project:

```
# .NET
bin/
obj/
*.user
*.suo

# Node / React
node_modules/
build/
dist/
.next/

# Source control / IDE
.git/
.vs/
.vscode/
.idea/

# Secrets and env
.env
.env.local
appsettings.Development.json

# OS / log noise
.DS_Store
Thumbs.db
*.log
```

Add a `.dockerignore` to every project. Always.

### Multi-Stage Builds

Default Dockerfiles produce huge images because everything you needed to build the app -- compilers, SDKs, dev dependencies -- is also in the final image.

A **multi-stage build** uses multiple `FROM` lines. Each stage is a temporary build environment; only the final stage produces the shipped image. You `COPY --from=<stage>` artifacts forward.

A canonical multi-stage Dockerfile for an ASP.NET Core app:

```dockerfile
# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file first to maximize cache hits.
COPY ProductCatalog.csproj .
RUN dotnet restore

# Now copy the rest and publish.
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Pull only the published output forward; SDK is left behind.
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductCatalog.dll"]
```

The build stage is ~700 MB (SDK + sources + intermediate output). The runtime stage that ships is ~210 MB (just the runtime + your published output). The difference is what multi-stage buys you.

### Layer Caching Strategy

The daemon caches each instruction's output. On rebuild, it reuses cached layers as long as the inputs to that instruction have not changed. The first invalidated instruction busts the cache for everything below it.

Two rules to design around this:

1. **Order instructions least-likely-to-change to most-likely-to-change.** Put system package installs near the top; copy your source code near the bottom.
2. **Copy dependency manifests separately from source code.** This is the cache-friendly pattern:

```dockerfile
# Copy only the project file. Restore depends only on this.
COPY ProductCatalog.csproj .
RUN dotnet restore   # cached as long as the .csproj does not change

# Copy the rest of the source. Edits here do NOT invalidate restore.
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore
```

The same pattern applies to Node.js (`COPY package*.json .` -> `RUN npm ci` -> `COPY . .`) and Python (`COPY requirements.txt .` -> `RUN pip install` -> `COPY . .`).

If you do `COPY . .` first and `RUN dotnet restore` second, every code edit invalidates the restore cache and reinstalls every package. Fix the order and rebuilds drop from minutes to seconds.

## Code Example

A complete multi-stage Dockerfile for the React app side, mirroring the .NET example above:

```dockerfile
# ---- Stage 1: build ----
FROM node:20-alpine AS build
WORKDIR /app

# Cache-friendly: copy manifests first, install, then copy source.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- Stage 2: serve ----
FROM nginx:1.25-alpine AS serve
COPY --from=build /app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

Build, tag, push:

```bash
docker build -t myuser/productcatalog-web:1.0.0 .
docker run -d -p 8080:80 --name web myuser/productcatalog-web:1.0.0
docker push myuser/productcatalog-web:1.0.0
```

## Summary

- Docker Hub is the default registry; `library/<image>` is the official namespace, `username/<image>` is yours.
- Tags are conventions -- `:latest`, semver, and git SHA are the three you will see most.
- The Dockerfile instructions you need daily: `FROM`, `WORKDIR`, `COPY`, `RUN`, `ENV`, `EXPOSE`, `CMD`, `ENTRYPOINT`.
- Use `CMD` alone when the command is overridable; use `ENTRYPOINT` (with optional `CMD` defaults) when the container wraps one fixed program. Always use exec form.
- Ship a `.dockerignore` with every Dockerfile to shrink the build context and keep secrets out.
- Multi-stage builds drop runtime image size by leaving SDKs and dev deps behind.
- Order instructions by change frequency and copy manifests before source to maximize layer cache hits.

## Additional Resources

- [Dockerfile reference (official)](https://docs.docker.com/engine/reference/builder/)
- [Best practices for writing Dockerfiles](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [Multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
