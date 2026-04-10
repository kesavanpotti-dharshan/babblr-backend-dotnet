# Babblr — Backend API

A high-performance, real-time messaging API built with **ASP.NET Core 8** following **Clean Architecture** principles. Deployed on Azure App Service with a GitHub Actions CI/CD pipeline.

> **Live API:** https://babblr-api.azurewebsites.net/swagger  
> **Frontend:** https://babblr-chat.vercel.app  
> **Frontend repo:** https://github.com/kesavanpotti-dharshan/babblr-frontend-react

---

## Features

- Real-time bidirectional messaging via **ASP.NET Core SignalR**
- Stateless **JWT authentication** with ASP.NET Core Identity
- **Room management** — create, join, leave, and discover public rooms
- **Message history** — paginated retrieval with load more support
- **Message actions** — edit, soft delete, and full-text search
- **File uploads** — images and documents via Azure Blob Storage
- **Presence tracking** — online/offline status with multi-tab awareness
- **Typing indicators** — real-time broadcast to room members
- **Global error handling** — RFC 7807 ProblemDetails responses
- **Health check** endpoint for Azure App Service monitoring

---

## Architecture

Babblr follows **Clean Architecture** with a strict inward dependency rule — outer layers depend on inner layers, never the reverse.

```
Babblr.API               → Host, controllers, SignalR hub, middleware
Babblr.Core              → Domain entities, interfaces, DTOs (zero external dependencies)
Babblr.Infrastructure    → EF Core, repositories, services, Azure integrations
Babblr.Shared            → Constants, helpers, error types
Babblr.Tests             → xUnit unit tests
```

### Dependency flow

```
API  →  Core  ←  Infrastructure
 ↑                     ↑
 └────── Shared ────────┘
```

`Babblr.Core` has zero external NuGet dependencies. All infrastructure concerns are hidden behind interfaces defined in Core and implemented in Infrastructure.

### Key patterns

**Repository pattern** — controllers and services never touch `DbContext` directly. All data access goes through typed repository interfaces (`IRoomRepository`, `IMessageRepository`, `IRoomMemberRepository`).

**Unit of Work** — all repositories share a single `DbContext` per request. `SaveChangesAsync()` commits everything atomically.

**Interface-driven services** — `IPresenceTracker`, `IStorageService`, `ITokenService`, and `IAuthService` are defined in Core. Swapping implementations is a single DI registration change in `Program.cs`. For example, swapping `InMemoryPresenceTracker` for `RedisPresenceTracker` requires changing one line.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Real-time | ASP.NET Core SignalR |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (Supabase) |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| File Storage | Azure Blob Storage |
| Presence Tracking | In-memory (Redis-ready interface) |
| Hosting | Azure App Service (Linux) |
| CI/CD | GitHub Actions |
| API Docs | Swagger / Swashbuckle |
| Testing | xUnit + Moq + FluentAssertions |

---

## Project Structure

```
Babblr/
├── Babblr.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── RoomsController.cs
│   │   ├── MessagesController.cs
│   │   ├── UploadsController.cs
│   │   └── UsersController.cs
│   ├── Hubs/
│   │   └── ChatHub.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
├── Babblr.Core/
│   ├── Entities/
│   │   ├── AppUser.cs
│   │   ├── Room.cs
│   │   ├── Message.cs
│   │   └── RoomMember.cs
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IRepository.cs
│   │   │   ├── IRoomRepository.cs
│   │   │   ├── IMessageRepository.cs
│   │   │   ├── IRoomMemberRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── Services/
│   │       ├── IAuthService.cs
│   │       ├── ITokenService.cs
│   │       ├── IPresenceTracker.cs
│   │       └── IStorageService.cs
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Message/
│   │   └── Room/
│   └── Enums/
│       └── RoomRole.cs
├── Babblr.Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Repositories/
│   │   ├── BaseRepository.cs
│   │   ├── RoomRepository.cs
│   │   ├── MessageRepository.cs
│   │   ├── RoomMemberRepository.cs
│   │   └── UnitOfWork.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── TokenService.cs
│       ├── AzureBlobStorageService.cs
│       ├── InMemoryPresenceTracker.cs
│       └── RedisPresenceTracker.cs     ← stub, ready to wire up
├── Babblr.Shared/
├── Babblr.Tests/
│   └── Unit/
│       ├── Services/
│       │   ├── TokenServiceTests.cs
│       │   ├── AuthServiceTests.cs
│       │   └── PresenceTrackerTests.cs
│       └── Controllers/
│           └── AuthControllerTests.cs
└── .github/
    └── workflows/
        └── deploy.yml
```

---

## API Endpoints

### Auth
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login and receive JWT | No |
| GET | `/api/auth/me` | Get current user from token | Yes |

### Rooms
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/rooms` | Create a new room | Yes |
| GET | `/api/rooms` | Get rooms for current user | Yes |
| GET | `/api/rooms/{roomId}` | Get room details | Yes |
| GET | `/api/rooms/discover` | Get all public rooms | Yes |
| POST | `/api/rooms/{roomId}/join` | Join a public room | Yes |
| POST | `/api/rooms/{roomId}/leave` | Leave a room | Yes |

### Messages
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/messages/room/{roomId}` | Paginated message history | Yes |
| PUT | `/api/messages/{messageId}` | Edit a message | Yes |
| DELETE | `/api/messages/{messageId}` | Soft delete a message | Yes |
| GET | `/api/messages/room/{roomId}/search?q=` | Search messages | Yes |

### Users
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/users/me` | Get current user profile | Yes |
| PUT | `/api/users/me` | Update display name / avatar | Yes |
| GET | `/api/users/online` | Get online users | Yes |

### Uploads
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/uploads` | Upload file to Azure Blob Storage | Yes |

### Health
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/health` | Health check | No |

---

## SignalR Hub — `/hubs/chat`

Connect with JWT token as query string:
```
wss://babblr-api.azurewebsites.net/hubs/chat?access_token=YOUR_JWT
```

### Client → Server

| Method | Parameters | Description |
|---|---|---|
| `JoinRoom` | `roomId: string` | Join a chat room group |
| `LeaveRoom` | `roomId: string` | Leave a chat room group |
| `SendMessage` | `{ roomId, content }` | Send a message |
| `EditMessage` | `messageId, roomId, newContent` | Broadcast message edit |
| `DeleteMessage` | `messageId, roomId` | Broadcast message deletion |
| `TypingStarted` | `roomId: string` | Notify typing started |
| `TypingStopped` | `roomId: string` | Notify typing stopped |
| `GetOnlineUsers` | — | Request online user list |

### Server → Client

| Event | Payload | Description |
|---|---|---|
| `ReceiveMessage` | `{ messageId, content, senderId, roomId, sentAt }` | New message |
| `MessageEdited` | `{ messageId, newContent, editedAt }` | Message edited |
| `MessageDeleted` | `{ messageId, deletedAt }` | Message deleted |
| `UserJoined` | `{ userId, roomId, joinedAt }` | User joined room |
| `UserLeft` | `{ userId, roomId }` | User left room |
| `UserOnline` | `userId: string` | User came online |
| `UserOffline` | `userId: string` | User went offline |
| `UserTyping` | `{ userId, roomId }` | User is typing |
| `UserStoppedTyping` | `{ userId, roomId }` | User stopped typing |
| `OnlineUsers` | `string[]` | List of online user IDs |
| `Error` | `message: string` | Hub error |

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- PostgreSQL (or a free Supabase account)
- Azure Storage account (or use Azurite for local emulation)

### Setup

```bash
# Clone the repo
git clone https://github.com/kesavanpotti-dharshan/babblr-backend-dotnet.git
cd babblr-backend-dotnet

# Set user secrets
cd Babblr.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_POSTGRES_CONNECTION_STRING"
dotnet user-secrets set "Jwt:Key" "your-secret-key-min-32-characters"
dotnet user-secrets set "Jwt:Issuer" "babblr-api"
dotnet user-secrets set "Jwt:Audience" "babblr-client"
dotnet user-secrets set "Azure:BlobStorageConnection" "YOUR_AZURE_STORAGE_CONNECTION_STRING"
dotnet user-secrets set "Azure:BlobContainerName" "babblr-uploads"

# Run migrations (uses direct connection, not pooler)
dotnet ef database update --project ../Babblr.Infrastructure/Babblr.Infrastructure.csproj

# Run the API
dotnet run --launch-profile http
```

Open `http://localhost:5174/swagger` to explore the API.

### Running tests

```bash
dotnet test --verbosity normal
```

---

## CI/CD Pipeline

Every push to `main` triggers a GitHub Actions workflow:

```
push to main → Restore → Build → Test → Publish → Deploy → Live on Azure
```

The pipeline only deploys on push to `main` — pull requests run build and tests only.

---

## Design Decisions

**Why Clean Architecture?**
Business logic is completely independent of infrastructure. The entire `Babblr.Core` project can be tested without a database, HTTP server, or Azure service.

**Why SignalR over raw WebSockets?**
SignalR handles connection negotiation, fallback transports, reconnection, and group management out of the box. It integrates natively with ASP.NET Core's DI and authentication pipeline.

**Why soft delete for messages?**
Hard deletes create gaps in message history that break pagination and conversation context. Soft deletes preserve the timeline while hiding content — the same approach used by Slack and Discord.

**Why in-memory presence with a Redis interface?**
Presence data is ephemeral. The `IPresenceTracker` interface means swapping to Redis for multi-instance scale-out is a single line change in `Program.cs` with zero changes to business logic.

**Why Supabase for PostgreSQL?**
Free hosted PostgreSQL with a connection pooler, dashboard, and the same Npgsql driver used in production Azure deployments. Zero infrastructure overhead during development.

---

## Roadmap

- [ ] Redis presence tracking (Upstash free tier)
- [ ] Refresh token rotation
- [ ] Rate limiting per user per room
- [ ] Integration tests with Testcontainers
- [ ] Serilog structured logging with Azure Application Insights
- [ ] Direct messaging between users
- [ ] Message reactions

---

## Author

**Dharshan Kesavanpotti** — .NET backend developer based in USA  
[GitHub](https://github.com/kesavanpotti-dharshan) · [LinkedIn](https://www.linkedin.com/in/dharshankesavan/)