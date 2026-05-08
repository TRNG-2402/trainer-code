# Installing Docker

## Learning Objectives
- Install Docker Desktop on Windows (with WSL2) or macOS, or Docker Engine on Linux.
- Verify the install with `docker --version` and `docker run hello-world`.
- Navigate the Docker Desktop UI -- containers, images, volumes, settings.
- Identify the system requirements for each platform.
- Diagnose common install pitfalls before they cost you a day.

## Why This Matters

Every command you run for the rest of the week assumes a working Docker install. Most of the time you will spend "fixing Docker" is the install -- once it is up, it stays up. Get this right today and the rest of the week is friction-free.

## The Concept

### Two Distributions: Docker Desktop and Docker Engine

- **Docker Desktop** -- the GUI-bundled distribution for Windows and macOS. It includes the daemon, CLI, Docker Compose, Kubernetes, and a UI. On Windows, Docker Desktop runs `dockerd` inside a lightweight Linux VM provided by WSL2.
- **Docker Engine** -- the daemon + CLI only, installed via your Linux distribution's package manager. No GUI. This is what you run on a Linux server.

For this course on Windows or macOS, install **Docker Desktop**. On Linux, install **Docker Engine**.

### Installing Docker Desktop on Windows (WSL2 backend)

System requirements:

- Windows 10 64-bit (build 19044+) or Windows 11.
- Hardware virtualization enabled in BIOS/UEFI.
- WSL2 enabled with a Linux distribution installed (Ubuntu is the common default).
- 4 GB RAM minimum (8+ GB recommended).

Steps:

1. **Enable WSL2.** Open PowerShell as administrator and run:

   ```powershell
   wsl --install
   ```

   Reboot when prompted. This installs WSL, enables the Virtual Machine Platform feature, and installs Ubuntu by default.

2. **Download Docker Desktop** from `https://www.docker.com/products/docker-desktop/`.

3. **Run the installer.** Leave "Use WSL 2 instead of Hyper-V" checked. Reboot if asked.

4. **Launch Docker Desktop.** Accept the license. The whale icon in the system tray turns solid when the daemon is ready.

5. **Verify WSL2 integration.** In Docker Desktop: Settings -> Resources -> WSL Integration. Enable integration with your distro. This makes the `docker` CLI available inside WSL shells.

### Installing Docker Desktop on macOS

System requirements:

- macOS 12 (Monterey) or later for Apple silicon; macOS 12 or later for Intel.
- 4 GB RAM minimum.

Steps:

1. Download the right `.dmg` -- **Apple Silicon** for M1/M2/M3 Macs, **Intel chip** for older Macs. Mismatched architecture is the #1 install failure.
2. Drag Docker.app to Applications.
3. Launch Docker, authorize the privileged helper when prompted.
4. The whale icon appears in the menu bar; wait for it to settle.

### Installing Docker Engine on Linux (Ubuntu/Debian)

There is no GUI on Linux -- you install the daemon and CLI directly.

```bash
# Remove old packages if present.
sudo apt-get remove docker docker-engine docker.io containerd runc

# Install prerequisites and add Docker's official GPG key.
sudo apt-get update
sudo apt-get install ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | \
  sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Add the Docker apt repository.
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo $VERSION_CODENAME) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine, CLI, and Compose plugin.
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Run docker without sudo (log out and back in after this).
sudo usermod -aG docker $USER
```

Always check the official install page for your distro -- package names and repo URLs change.

### Verifying the Install

Run two commands. Both should succeed without errors:

```bash
# Show client + server versions. If "Cannot connect to the Docker daemon"
# appears, the daemon is not running.
docker --version
docker version

# Pull and run the smallest test image. It downloads, runs, prints a
# message, and exits.
docker run hello-world
```

If `hello-world` prints "Hello from Docker!", you are done. If not, the error message tells you what to fix.

### Docker Desktop UI Tour

When `docker run hello-world` works, open Docker Desktop and explore:

- **Containers pane** -- every container on the host, running or stopped. Click one to see logs, stats, files, and exec into it. The same data is available from `docker ps` and `docker logs`.
- **Images pane** -- every image stored locally. Shows size, tag, and last-used date. Right-click to run, push, or remove.
- **Volumes pane** -- managed persistent volumes. You will use these on Day 1 of any production deployment.
- **Settings (gear icon)** -- resource limits (CPU, memory), WSL integration on Windows, file sharing on macOS, registry sign-in. The default resource limits are conservative; raise them if builds feel slow.

The UI is a window onto the same daemon the CLI talks to. Anything you do in the UI is doable from the CLI and vice versa.

### Common Install Pitfalls

- **WSL2 not actually enabled.** "Docker Desktop requires WSL2" errors mean the Windows feature is missing. Run `wsl --status` and `wsl --update`.
- **Hardware virtualization disabled.** On Windows, `systeminfo | findstr Hyper-V` should show virtualization enabled. If not, enable VT-x/AMD-V in BIOS.
- **macOS architecture mismatch.** Installing the Intel build on Apple silicon runs through Rosetta and is painfully slow. Install the Apple Silicon build.
- **Old Docker Toolbox or Docker Machine** lingering on the system. Uninstall before installing Docker Desktop.
- **Permission denied on `/var/run/docker.sock`** on Linux. You did not log out and back in after `usermod -aG docker $USER`.
- **Corporate proxy blocks `registry-1.docker.io`.** Configure proxy in Docker Desktop Settings -> Resources -> Proxies, or in `/etc/docker/daemon.json` on Linux.
- **Disk full.** Images and layers add up. `docker system prune` reclaims space.

## Code Example

The verification ritual you should run on every fresh install:

```bash
# 1. Confirm CLI is on PATH and reports a version.
docker --version

# 2. Confirm daemon is reachable.
docker info

# 3. End-to-end test: pull, create, run, exit.
docker run hello-world

# 4. Confirm cleanup works.
docker ps -a               # see the exited hello-world container
docker rm $(docker ps -aq) # remove all stopped containers
docker rmi hello-world     # remove the image
```

If all four steps work, you have a fully functional install.

## Summary

- Docker Desktop (Windows/macOS, GUI included) and Docker Engine (Linux, headless) are the two distributions.
- On Windows, Docker Desktop uses the WSL2 backend; enable WSL2 first, then install.
- On macOS, match the installer architecture to your chip (Apple silicon vs Intel).
- On Linux, install via the official apt/dnf repository, then add your user to the `docker` group.
- Verify with `docker --version`, `docker info`, and `docker run hello-world`.
- Most install failures trace back to WSL2/virtualization, architecture mismatch, or proxy/firewall blocking the registry.

## Additional Resources

- [Install Docker Desktop on Windows](https://docs.docker.com/desktop/install/windows-install/)
- [Install Docker Desktop on Mac](https://docs.docker.com/desktop/install/mac-install/)
- [Install Docker Engine on Linux](https://docs.docker.com/engine/install/)
