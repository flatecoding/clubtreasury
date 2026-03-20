# ClubTreasury

ClubTreasury is a web-based administration tool for club treasurers. It helps manage members, transactions, budgets, and financial reports.

## Installation

ClubTreasury runs as a Docker container with a PostgreSQL database. Choose one of the two setup options below:

- [Direct Access](installation/direct-access.md) — simple setup, access the app directly via port
- [Behind a Reverse Proxy](installation/reverse-proxy.md) — recommended for production, uses a reverse proxy (e.g. Nginx Proxy Manager) for HTTPS and domain routing

## Requirements

- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/) installed on your system
- Minimum 512 MB RAM available for the containers
- A few hundred MB of disk space (plus storage for your data)

## Quick Start

```bash
# 1. Download the compose file
curl -O https://raw.githubusercontent.com/flatecoding/clubtreasury/main/compose.yaml

# 2. Edit the file and set your passwords
nano compose.yaml

# 3. Start the application
docker compose up -d

# 4. Open in your browser
# http://localhost:8080
```