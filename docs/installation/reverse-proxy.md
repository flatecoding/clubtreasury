# Reverse Proxy Setup

This setup is recommended for production. The application runs behind a reverse proxy (e.g. Nginx Proxy Manager) which handles HTTPS and domain routing. The app and database are isolated in an internal network.

## Prerequisites

- A running reverse proxy (e.g. [Nginx Proxy Manager](https://nginxproxymanager.com/))
- An external Docker network named `proxy` that your reverse proxy is connected to

If you haven't created the proxy network yet:

```bash
docker network create proxy
```

## Step 1: Download the Compose File

Download the `compose.npm.yaml` from the repository:

```bash
curl -O https://raw.githubusercontent.com/flatecoding/clubtreasury/main/compose.npm.yaml
```

Or copy the content manually:

```yaml
services:
  postgres:
    image: postgres:17
    container_name: clubtreasury-db
    restart: always
    environment:
      POSTGRES_PASSWORD: YourSecureDbPassword
      POSTGRES_DB: ClubCash
      POSTGRES_USER: dev
    volumes:
      - ./clubtreasury/postgres:/var/lib/postgresql/data
    networks:
      - clubtreasury-backend
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U dev -d ClubCash"]
      start_period: 10s
      interval: 10s
      timeout: 5s
      retries: 5

  webapp:
    container_name: clubtreasury-web
    image: flatecoding/clubtreasury:latest
    restart: always
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      UI_APPNAME: ClubTreasury
      UI_HOMEDESCRIPTION: Administrationtool for club treasures
      UI_APPNAMENAVBAR: ClubTreasury
      Logging__LogLevel__Default: "Information"
      Logging__LogLevel__Microsoft.EntityFrameworkCore: "Information"
      ASPNETCORE_ENVIRONMENT: "Production"
      DbName: ClubCash
      DbUser: YourDbUser
      DbPassword: YourSecureDbPassword
      ADMIN_USERNAME: admin
      ADMIN_EMAIL: admin@admin.de
      ADMIN_PASSWORD: YourSecureAdminPassword
    volumes:
      - ./clubtreasury/exports:/app/exports
      - ./clubtreasury/logs:/app/logs
    networks:
      - clubtreasury-backend
      - proxy

networks:
  clubtreasury-backend:
    driver: bridge
  proxy:
    external: true
```

## Step 2: Configure Credentials

Edit the `compose.npm.yaml` and set the following values:

| Variable | Description |
|---|---|
| `POSTGRES_DB` | Name of the PostgreSQL database |
| `POSTGRES_USER` | Username for the PostgreSQL database |
| `POSTGRES_PASSWORD` | Password for the PostgreSQL database |
| `DbName` | Must match `POSTGRES_DB` |
| `DbUser` | Must match `POSTGRES_USER` |
| `DbPassword` | Must match `POSTGRES_PASSWORD` |
| `ADMIN_USERNAME` | Username for the initial admin account |
| `ADMIN_EMAIL` | Email for the initial admin account |
| `ADMIN_PASSWORD` | Password for the initial admin account |

## Step 3: Configure UI Settings (Optional)

| Variable | Description | Default |
|---|---|---|
| `UI_APPNAME` | Application title shown in the browser tab | ClubTreasury |
| `UI_HOMEDESCRIPTION` | Description shown on the home page | — |
| `UI_APPNAMENAVBAR` | Name shown in the navigation bar | ClubTreasury |

## Step 4: Start the Application

```bash
docker compose -f compose.npm.yaml up -d
```

## Step 5: Configure the Reverse Proxy

In your reverse proxy (e.g. Nginx Proxy Manager), create a new proxy host:

| Setting | Value |
|---|---|
| Domain | `clubtreasury.yourdomain.com` |
| Forward Hostname | `clubtreasury-web` |
| Forward Port | `8080` |
| Websockets Support | Enabled |
| SSL | Request a new SSL certificate |

> **Important:** Enable Websockets Support — ClubTreasury uses Blazor Server which requires a persistent SignalR connection.

## Step 6: Access the Application

Open your browser and navigate to your configured domain:

```
https://clubtreasury.yourdomain.com
```

Log in with the admin credentials you configured in Step 2.

## Network Architecture

```
Internet
  │
  ▼
Reverse Proxy (proxy network)
  │
  ▼
clubtreasury-web (proxy + clubtreasury-backend networks)
  │
  ▼
clubtreasury-db (clubtreasury-backend network only)
```

The database is only accessible from the webapp container — it is not exposed to the host or the internet.

## Useful Commands

```bash
# View logs
docker compose -f compose.npm.yaml logs -f

# Stop the application
docker compose -f compose.npm.yaml down

# Update to the latest version
docker compose -f compose.npm.yaml pull
docker compose -f compose.npm.yaml up -d
```

## Debugging Database Access

If you need direct access to the database for debugging, uncomment the ports section in `compose.npm.yaml`:

```yaml
ports:
  - "5433:5432"
```

Then restart with `docker compose -f compose.npm.yaml up -d`. Remember to comment it out again when done.