# The Docker CLI

## Learning Objectives
- Use the core Docker CLI commands fluently: `pull`, `images`, `run`, `ps`, `logs`, `exec`, `stop`/`start`/`restart`, `rm`, `rmi`, `system prune`, `inspect`.
- Apply the most useful `docker run` flags: `-d`, `-p`, `--name`, `-e`, `-v`, `--rm`.
- Sequence common workflows: run a container, read its logs, exec in to debug, clean up.
- Recognize when each cleanup command is appropriate and what it removes.

## Why This Matters

The Docker CLI is the daily driver for everything containerized: development, debugging, CI scripts, ops triage. Every other Docker tool (Compose, Kubernetes, CI runners) eventually maps back to these primitives. Once these commands are reflex, you can debug any container-based system.

## The Concept

### Pulling and Inspecting Images

```bash
# Download an image from a registry into the local image store.
docker pull nginx:1.25-alpine

# List local images.
docker images
# or the modern equivalent
docker image ls

# Look inside an image -- env, cmd, exposed ports, layers, digest.
docker inspect nginx:1.25-alpine
```

`docker pull` does network work; `docker images` is local-only.

### Running Containers: `docker run` and Its Flags

`docker run` is the most-used command in Docker, and the one with the most flags. Six flags cover 95% of daily work:

- **`-d` (detached).** Run the container in the background and return the prompt immediately. Without `-d`, your terminal is attached to the container's stdout/stderr.
- **`-p HOST:CONTAINER` (port).** Map a host port to a container port. `-p 8080:80` exposes the container's port 80 on the host's 8080. Repeat the flag to publish multiple ports.
- **`--name`.** Give the container a stable, human-readable name. Without this, Docker generates a random name like `eager_einstein`.
- **`-e KEY=VALUE` (env).** Set an environment variable in the container. Repeat per variable, or use `--env-file` for a file.
- **`-v HOSTPATH:CONTAINERPATH` (volume / bind mount).** Mount a host directory or named volume into the container. Use for persistent data, configuration, or live code mounts in development.
- **`--rm` (autoremove).** Remove the container automatically when it exits. Great for one-shot commands; do not use for long-running services where you want logs after a crash.

A typical web server invocation pulls all six together:

```bash
docker run -d \
  -p 8080:80 \
  --name web \
  -e NGINX_HOST=example.com \
  -v $PWD/site:/usr/share/nginx/html:ro \
  --rm \
  nginx:1.25-alpine
```

Detached, port-mapped, named, env-configured, content mounted read-only from the host, auto-removed on exit.

### Volumes: Bind Mounts, Named Volumes, and Build-via-Container

The `-v` flag mounts storage into a container. There are two flavors and one common workflow built on top of them.

**Bind mount** — mount a host path into the container. Format: `-v <ABSOLUTE_HOST_PATH>:<CONTAINER_PATH>`. Use for live source code in dev, host config files, or pulling build output back to the host.

```bash
# Linux / macOS
docker run -v /home/me/data:/app/data myimage

# Windows (PowerShell)
docker run -v C:\data:/app/data myimage

# Read-only mount (recommended when the container should not modify the host)
docker run -v $PWD/site:/usr/share/nginx/html:ro nginx
```

**Named volume** — a Docker-managed volume stored under the daemon's data directory. Format: `-v <VOLUME_NAME>:<CONTAINER_PATH>`. Use for databases and any persistent state where you do not need direct host access.

```bash
# 1. Create the volume.
docker volume create mydata

# 2. Mount it into a container.
docker run -d --name db -v mydata:/var/lib/postgresql/data postgres:16

# 3. Inspect / list / remove.
docker volume ls
docker volume inspect mydata
docker volume rm mydata          # only when no container references it
```

Stop and remove the container; the named volume survives. Start a new container with the same volume and the data is still there.

**Build-via-container pattern.** Mount the host project directory into a build-tool image, run the build inside the container, and the artifacts land back on the host through the same mount. No SDK install on the host required.

```bash
# Build a Node project inside a node:20 container; output goes to the host's dist/.
docker run --rm \
  -v ${PWD}:/src \
  -w /src \
  node:20 \
  npm run build

# Build a .NET project the same way.
docker run --rm \
  -v ${PWD}:/src \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet publish -c Release -o /src/publish
```

Three flags do the work: `--rm` removes the container after exit, `-v ${PWD}:/src` bind-mounts the current directory, `-w /src` sets the working directory inside the container. This is how CI runners, polyglot dev environments, and "no SDK on my machine" teams build artifacts cleanly.

### Listing and Inspecting Containers

```bash
# Running containers only.
docker ps

# All containers, including stopped/exited.
docker ps -a

# Just IDs (useful for scripting).
docker ps -aq

# Container details: networks, mounts, env, state, restart policy, last error.
docker inspect web

# Real-time CPU, memory, network, I/O.
docker stats

# Filter by name, status, etc.
docker ps -a --filter "status=exited" --filter "name=web"
```

`docker ps -a` is your first move when "the container disappeared" -- usually it exited, and `ps -a` shows you why.

### Reading Logs

```bash
# All logs the container has emitted to stdout/stderr.
docker logs web

# Stream logs as they arrive (Ctrl-C to stop).
docker logs -f web

# Last N lines, then follow.
docker logs --tail 100 -f web

# With timestamps.
docker logs -t web
```

Anything your app writes to stdout/stderr is captured by the daemon and surfaces here. Apps that log to files inside the container are invisible to `docker logs` -- prefer stdout for containerized apps.

### Executing Commands Inside a Running Container

```bash
# Open an interactive shell in the container.
docker exec -it web sh
# or, if bash is installed:
docker exec -it web bash

# Run a one-shot command and return.
docker exec web cat /etc/nginx/nginx.conf

# Run as a specific user.
docker exec -u root -it web sh
```

`-i` keeps stdin open; `-t` allocates a TTY. Together they give you an interactive shell. This is your debugging life raft -- use it to inspect filesystems, processes (`ps aux`), config files, and network state inside the container.

Note: alpine-based images ship with `sh`, not `bash`. Use `sh` if `bash` is missing.

### Stopping, Starting, Restarting

```bash
docker stop web        # SIGTERM, wait 10s, SIGKILL
docker start web       # restart a stopped container (writable layer preserved)
docker restart web     # stop then start
docker kill web        # immediate SIGKILL, no graceful shutdown
docker pause web       # freeze processes via cgroups
docker unpause web
```

`stop` is graceful; `kill` is immediate. Prefer `stop` so the app can shut down cleanly.

### Removing Containers and Images

```bash
# Remove a stopped container.
docker rm web

# Force-remove a running container (stop + remove).
docker rm -f web

# Remove an image (only if no containers reference it).
docker rmi nginx:1.25-alpine

# Remove all stopped containers.
docker rm $(docker ps -aq)
```

You cannot remove an image while a container (running or stopped) still references it -- remove the container first.

### Cleaning Up: `docker system prune`

Docker hoards disk by design (cached layers and stopped containers speed up rebuilds). Periodically reclaim space:

```bash
# Remove stopped containers, dangling images, unused networks, build cache.
docker system prune

# Also remove unused images (not just dangling ones).
docker system prune -a

# Also remove unused volumes (DANGEROUS -- this can delete your data).
docker system prune -a --volumes

# Targeted cleanups.
docker container prune
docker image prune
docker volume prune
docker network prune

# See what is using disk.
docker system df
```

`prune --volumes` removes any volume not currently mounted by a container. If you stopped a database container and ran `prune --volumes`, the data is gone. Be deliberate.

### Common Workflows

Three sequences cover most daily Docker work:

**Workflow 1 -- Run a service and check it.**

```bash
docker run -d -p 8080:80 --name web nginx:1.25-alpine
curl http://localhost:8080
docker logs web
```

**Workflow 2 -- Debug a misbehaving container.**

```bash
docker ps -a                       # find the container, see status
docker logs --tail 200 web         # check recent logs
docker exec -it web sh             # exec in
ps aux                             # what is running?
cat /etc/nginx/nginx.conf          # verify config
exit
docker inspect web                 # check mounts, env, network
```

**Workflow 3 -- Clean up after a working session.**

```bash
docker stop $(docker ps -q)        # stop all running containers
docker rm $(docker ps -aq)         # remove all containers
docker image prune                 # drop dangling images
docker system df                   # see what is left
```

## Code Example

End-to-end run, debug, clean from a single shell session:

```bash
# Run a redis container.
docker run -d --name cache -p 6379:6379 redis:7-alpine

# Confirm it is up.
docker ps

# Watch logs in another terminal (or use -f here briefly).
docker logs cache

# Open the redis CLI inside the container.
docker exec -it cache redis-cli
# At the prompt:
# > PING
# > SET hello world
# > GET hello
# > exit

# Inspect the container's IP and mounts.
docker inspect cache --format '{{json .NetworkSettings.IPAddress}}'

# Stop and remove.
docker stop cache
docker rm cache

# Drop the image.
docker rmi redis:7-alpine
```

## Summary

- `docker run` with `-d -p --name -e -v --rm` covers most container starts.
- `docker ps -a`, `docker logs`, and `docker exec -it ... sh` are the debugging trio.
- `docker stop` is graceful; `docker kill` is immediate.
- You must `rm` containers before you can `rmi` the image.
- `docker system prune` reclaims disk; `--volumes` is destructive -- be sure first.

## Additional Resources

- [Docker CLI reference](https://docs.docker.com/engine/reference/commandline/cli/)
- [`docker run` reference](https://docs.docker.com/engine/reference/commandline/run/)
- [Pruning unused Docker objects](https://docs.docker.com/config/pruning/)
