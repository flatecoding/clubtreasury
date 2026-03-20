# Direct Access Setup

This is the simplest setup. The application is accessible directly via a port on your host machine.

## Step 1: Download the Compose File

Download the `compose.yaml` from the repository:

```bash
curl -O https://raw.githubusercontent.com/flatecoding/clubtreasury/main/compose.yaml
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
      POSTGRES_USER: user
    volumes:
      - ./clubtreasury/postgres:/var/lib/postgresql/data
    ports:
      - "5433:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U user -d ClubCash"]
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
      Logging__LogLevel__Default: "Information"
      Logging__LogLevel__Microsoft.EntityFrameworkCore: "Information"
      ASPNETCORE_ENVIRONMENT: "Production"
      DbPassword: YourSecureDbPassword
      ADMIN_USERNAME: admin
      ADMIN_EMAIL: admin@admin.de
      ADMIN_PASSWORD: YourSecureAdminPassword
    volumes:
      - ./clubtreasury/exports:/app/exports
      - ./clubtreasury/logs:/app/logs
    ports:
      - "8080:8080"
```

## Step 2: Configure Passwords

Edit the `compose.yaml` and set the following values:

| Variable | Description |
|---|---|
| `POSTGRES_PASSWORD` | Password for the PostgreSQL database |
| `DbPassword` | Must match `POSTGRES_PASSWORD` |
| `ADMIN_USERNAME` | Username for the initial admin account |
| `ADMIN_EMAIL` | Email for the initial admin account |
| `ADMIN_PASSWORD` | Password for the initial admin account |

## Step 3: Adjust Volume Paths (Optional)

By default, data is stored in `./clubtreasury/` relative to the compose file. Change the volume paths if you want to store data elsewhere:

```yaml
volumes:
  - /your/custom/path/postgres:/var/lib/postgresql/data
```

## Step 4: Start the Application

```bash
docker compose up -d
```

This downloads the images and starts both containers. The PostgreSQL database will be initialized automatically.

## Step 5: Access the Application

Open your browser and navigate to:

```
http://localhost:8080
```

Log in with the admin credentials you configured in Step 2.

## Useful Commands

```bash
# View logs
docker compose logs -f

# Stop the application
docker compose down

# Update to the latest version
docker compose pull
docker compose up -d

# Restart
docker compose restart
```

## Troubleshooting

**Application won't start?**
Check the logs with `docker compose logs webapp` — the most common issue is a mismatched `DbPassword` and `POSTGRES_PASSWORD`.

**Database connection error?**
Make sure the PostgreSQL container is healthy: `docker compose ps`. It may take a few seconds to initialize on first start.

**Port 8080 already in use?**
Change the port mapping in `compose.yaml`, e.g. `"9090:8080"`, then access via `http://localhost:9090`.