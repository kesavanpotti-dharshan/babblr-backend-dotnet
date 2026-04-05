# Babblr 💬

A high-performance, real-time messaging API built with **ASP.NET Core 8** and **SignalR**, following **Clean Architecture** principles. Built as a portfolio project to demonstrate production-grade .NET backend development.

---

## Live Demo

> API is deployed on **Azure App Service**.  
> Swagger UI available at the live URL — contact me for access.

---

## Features

- **Real-time messaging** — bidirectional WebSocket communication via SignalR
- **JWT authentication** — stateless, secure register and login flow
- **Room management** — create public or private rooms, join and leave dynamically
- **Message history** — paginated retrieval of past messages per room
- **Message actions** — edit and soft-delete messages with real-time broadcast to room members
- **Message search** — full-text search across messages within a room
- **File uploads** — upload images and documents via Azure Blob Storage
- **Presence tracking** — online/offline user status with multi-tab awareness
- **Typing indicators** — real-time "user is typing" events broadcast to room members
- **User profiles** — view and update display name and avatar URL

---

## Architecture

Babblr follows **Clean Architecture** with a strict inward dependency rule — outer layers depend on inner layers, never the reverse.

```
Babblr.API               → Host, controllers, SignalR hub, middleware
Babblr.Core              → Domain entities, interfaces, DTOs (no external dependencies)
Babblr.Infrastructure    → EF Core, repositories, services, Azure integrations
Babblr.Shared            → Constants, helpers, error types
Babblr.Tests             → xUnit test project
```

### Dependency flow

```
API → Core ← Infrastructure
 ↑                 ↑
 └──── Shared ─────┘
```

`Babblr.Core` has zero external NuGet dependencies. All infrastructure concerns — database, storage, identity — are hidden behind interfaces defined in Core and implemented in Infrastructure.

### Key patterns used

**Repository pattern** — controllers and services never touch `DbContext` directly. All data access goes through typed repository interfaces (`IRoomRepository`, `IMessageRepository`, `IRoomMemberRepository`).

**Unit of Work** — all repositories share a single `DbContext` instance per request. `SaveChangesAsync()` commits everything atomically, preventing partial writes.

**Interface-driven services** — `IPresenceTracker`, `IStorageService`, `ITokenService`, and `IAuthService` are all defined in Core. Swapping implementations (e.g. `InMemoryPresenceTracker` → `RedisPresenceTracker`) is a single DI registration change in `Program.cs`.

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
│       └── RedisPresenceTracker.cs (stub — ready for Upstash/Azure Cache)
├── Babblr.Shared/
│   ├── Constants/
│   ├── Helpers/
│   └── Errors/
└── Babblr.Tests/
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

### Messages
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/messages/room/{roomId}` | Get paginated message history | Yes |
| PUT | `/api/messages/{messageId}` | Edit a message | Yes |
| DELETE | `/api/messages/{messageId}` | Soft delete a message | Yes |
| GET | `/api/messages/room/{roomId}/search?q=` | Search messages in a room | Yes |

### Users
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/users/me` | Get current user profile | Yes |
| PUT | `/api/users/me` | Update display name / avatar | Yes |
| GET | `/api/users/online` | Get list of online users | Yes |

### Uploads
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/uploads` | Upload a file to Azure Blob Storage | Yes |

### Health
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/health` | Health check | No |

---

## SignalR Hub — `/hubs/chat`

Connect with a JWT token passed as a query string parameter:

```
wss://your-api/hubs/chat?access_token=YOUR_JWT_TOKEN
```

### Client → Server events

| Method | Parameters | Description |
|---|---|---|
| `JoinRoom` | `roomId: string` | Join a chat room group |
| `LeaveRoom` | `roomId: string` | Leave a chat room group |
| `SendMessage` | `{ roomId, content }` | Send a message to a room |
| `EditMessage` | `messageId, roomId, newContent` | Broadcast message edit to room |
| `DeleteMessage` | `messageId, roomId` | Broadcast message deletion to room |
| `TypingStarted` | `roomId: string` | Notify room members you are typing |
| `TypingStopped` | `roomId: string` | Notify room members you stopped typing |
| `GetOnlineUsers` | — | Request current online user list |

### Server → Client events

| Event | Payload | Description |
|---|---|---|
| `ReceiveMessage` | `{ messageId, content, senderId, roomId, sentAt }` | New message received |
| `MessageEdited` | `{ messageId, newContent, editedAt }` | Message was edited |
| `MessageDeleted` | `{ messageId, deletedAt }` | Message was soft deleted |
| `UserJoined` | `{ userId, roomId, joinedAt }` | User joined the room |
| `UserLeft` | `{ userId, roomId }` | User left the room |
| `UserOnline` | `userId: string` | User came online |
| `UserOffline` | `userId: string` | User went offline |
| `UserTyping` | `{ userId, roomId }` | User is typing |
| `UserStoppedTyping` | `{ userId, roomId }` | User stopped typing |
| `OnlineUsers` | `string[]` | List of currently online user IDs |
| `Error` | `message: string` | Hub error message |

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- Docker (optional, for local PostgreSQL)
- A Supabase account (free) or local PostgreSQL instance

### Setup

```bash
# Clone the repo
git clone https://github.com/kesavanpotti-dharshan/Babblr.git
cd Babblr

# Set up user secrets
cd Babblr.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_POSTGRES_CONNECTION_STRING"
dotnet user-secrets set "Jwt:Key" "your-secret-key-min-32-characters"
dotnet user-secrets set "Jwt:Issuer" "babblr-api"
dotnet user-secrets set "Jwt:Audience" "babblr-client"
dotnet user-secrets set "Azure:BlobStorageConnection" "YOUR_AZURE_STORAGE_CONNECTION_STRING"
dotnet user-secrets set "Azure:BlobContainerName" "babblr-uploads"

# Run migrations
dotnet ef database update --project ../Babblr.Infrastructure/Babblr.Infrastructure.csproj

# Run the API
dotnet run
```

Open `https://localhost:{PORT}/swagger` to explore the API.

---

## CI/CD Pipeline

Every push to `main` triggers a GitHub Actions workflow that:

1. Restores NuGet packages
2. Builds in Release mode
3. Runs the test suite
4. Publishes the API
5. Deploys to Azure App Service

```
push to main → Build → Test → Publish → Deploy → Live on Azure
```

---

## Design Decisions

**Why Clean Architecture?**  
Keeps business logic completely independent of infrastructure. The entire `Babblr.Core` project can be tested without a database, HTTP server, or any Azure service running.

**Why SignalR over raw WebSockets?**  
SignalR handles connection negotiation, fallback transports, reconnection logic, and group management out of the box. It also integrates natively with ASP.NET Core's dependency injection and authentication pipeline.

**Why soft delete for messages?**  
Hard deletes create gaps in message history that break pagination and conversation context. Soft deletes preserve the timeline while hiding content — the same approach used by Slack and Discord.

**Why in-memory presence tracker with a Redis interface?**  
Presence data is ephemeral and doesn't need to survive restarts. The `IPresenceTracker` interface means swapping to Redis (for multi-instance scale-out) is a single line change in `Program.cs` with zero changes to business logic.

**Why Supabase for PostgreSQL?**  
Free hosted PostgreSQL with a built-in dashboard, connection pooler, and the same Npgsql driver used in production Azure deployments. Zero infrastructure overhead during development.

---

## Roadmap

- [ ] Unit and integration tests (xUnit + Testcontainers)
- [ ] Redis presence tracking (Upstash free tier)
- [ ] Refresh token rotation
- [ ] Rate limiting per user per room
- [ ] React frontend client
- [ ] Direct messaging between users
- [ ] Message reactions

---

## Author

**Dharshan** — .NET backend developer  
[GitHub](https://github.com/kesavanpotti-dharshan) · [LinkedIn](https://linkedin.com/in/dharshankesavan/)
```