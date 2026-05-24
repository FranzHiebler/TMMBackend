# Deployment

## Railway Backend

Set these environment variables in Railway:

```text
ASPNETCORE_ENVIRONMENT=Production
MongoDb__ConnectionString=<MongoDB Atlas connection string>
MongoDb__DatabaseName=tmm
MongoDb__GamesCollectionName=gameSessions
Cors__AllowedOrigins__0=https://<frontend-domain>
Admin__UserIds__0=<admin-user-id>
```

Health check:

```text
GET /health
```

## Frontend

For Vercel or Railway static hosting:

```text
VITE_API_BASE_URL=https://<backend-domain>/api
```

Build command:

```text
npm run build
```

Output directory:

```text
dist
```

## MongoDB Atlas

- Add the Railway backend outbound IP or use the Atlas allowlist needed for the first demo.
- Store the connection string only in environment variables for live deployments.
- Indexes are initialized by `MongoIndexInitializer` on backend startup.
