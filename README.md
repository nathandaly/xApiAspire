# xAPI Learning Record Store (LRS) - .NET Aspire Application

A modern, distributed .NET 10 application implementing an xAPI (Experience API) Learning Record Store with a real-time monitoring dashboard, built using .NET Aspire for service orchestration.

## 🏗️ Architecture Overview

This project is built as a **.NET Aspire** distributed application, consisting of three main components:

1. **xApiApp.ApiService** - The xAPI Learning Record Store API
2. **xApiApp.Web** - Real-time dashboard frontend
3. **xApiApp.AppHost** - Aspire orchestrator that manages and connects services

### Technology Stack

- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API and Blazor Server
- **.NET Aspire** - Service orchestration and discovery
- **Tailwind CSS** - Modern, utility-first CSS framework
- **Blazor Server** - Interactive web UI with real-time updates

## 📁 Project Structure

```
xApiApp/
├── xApiApp.ApiService/          # xAPI LRS API Service
│   ├── Models/                  # xAPI data models
│   │   ├── Statement.cs
│   │   ├── Actor.cs
│   │   ├── Verb.cs
│   │   ├── StatementObject.cs
│   │   ├── Result.cs
│   │   ├── Context.cs
│   │   └── Attachment.cs
│   ├── Services/                # Business logic
│   │   ├── IStatementService.cs
│   │   └── StatementService.cs
│   ├── Middleware/              # HTTP middleware
│   │   └── XApiVersionMiddleware.cs
│   └── Program.cs               # API endpoints
│
├── xApiApp.Web/                 # Blazor Dashboard Frontend
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor  # Main dashboard page
│   │   └── Layout/
│   │       └── MainLayout.razor
│   ├── Services/
│   │   └── XApiClient.cs        # API client service
│   ├── Models/                  # Frontend models
│   │   └── Statement.cs
│   └── Program.cs
│
├── xApiApp.AppHost/             # Aspire Orchestrator
│   └── AppHost.cs               # Service configuration
│
└── xApiApp.ServiceDefaults/     # Shared service defaults
    └── Extensions.cs
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

### Installation

1. **Install the Aspire workload** (if not already installed):
   ```bash
   dotnet workload install aspire
   ```

2. **Clone and navigate to the project**:
   ```bash
   cd xApiApp
   ```

3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

### Running the Application

Run the entire application stack using Aspire:

```bash
aspire run
```

This command will:
- Build all projects
- Start the AppHost orchestrator
- Launch the API service
- Launch the web frontend
- Open the Aspire dashboard in your browser

The Aspire dashboard provides:
- **Service Discovery** - View all running services and their endpoints
- **Health Monitoring** - Real-time health status of all services
- **Logs** - Centralized logging from all services
- **Metrics** - Performance metrics and telemetry
- **Traces** - Distributed tracing information

### Accessing the Application

- **Web Dashboard**: `http://localhost:5000` (or port shown in Aspire dashboard)
- **API Service**: `http://localhost:5532` (HTTP) or `https://localhost:7395` (HTTPS)
- **Aspire Dashboard**: Automatically opens in browser, typically `http://localhost:15000`

## 📊 xAPI Structure

### What is xAPI?

The Experience API (xAPI) is a specification for tracking learning experiences. It allows learning systems to capture data about a wide range of learning activities and store them in a Learning Record Store (LRS).

### Core xAPI Components

#### 1. **Statement**

A Statement is the core data structure in xAPI. It represents a learning experience and follows the pattern: **"Actor Verb Object"**.

```json
{
  "id": "12345678-1234-5678-1234-567812345678",
  "actor": {
    "objectType": "Agent",
    "mbox": "mailto:learner@example.com",
    "name": "John Doe"
  },
  "verb": {
    "id": "http://adlnet.gov/expapi/verbs/completed",
    "display": {
      "en-US": "completed"
    }
  },
  "object": {
    "objectType": "Activity",
    "id": "http://example.com/xapi/activities/course1"
  },
  "result": {
    "success": true,
    "completion": true,
    "score": {
      "scaled": 0.95,
      "raw": 95,
      "min": 0,
      "max": 100
    }
  },
  "timestamp": "2024-01-15T10:30:00Z",
  "stored": "2024-01-15T10:30:05Z"
}
```

#### 2. **Actor**

Represents who performed the action. Can be:
- **Agent** - An individual (person or system)
- **Group** - A collection of Agents

Actors are identified using Inverse Functional Identifiers (IFIs):
- `mbox` - Email address (mailto: format)
- `mbox_sha1sum` - SHA1 hash of email
- `openid` - OpenID URL
- `account` - Account object with homePage and name

#### 3. **Verb**

Describes the action performed. Verbs are identified by IRIs and include display names in multiple languages.

Common verbs:
- `http://adlnet.gov/expapi/verbs/experienced`
- `http://adlnet.gov/expapi/verbs/completed`
- `http://adlnet.gov/expapi/verbs/passed`
- `http://adlnet.gov/expapi/verbs/failed`

#### 4. **Object**

The thing that was acted upon. Can be:
- **Activity** - A learning activity (course, quiz, video, etc.)
- **Agent** - Another person/system
- **Group** - A group of people
- **StatementRef** - Reference to another statement
- **SubStatement** - A nested statement

#### 5. **Result** (Optional)

Provides details about the outcome:
- `success` - Boolean indicating success/failure
- `completion` - Boolean indicating completion
- `score` - Score object with scaled/raw/min/max
- `duration` - ISO 8601 duration string
- `response` - Response string

#### 6. **Context** (Optional)

Provides additional context:
- `registration` - UUID for grouping statements
- `instructor` - Agent who instructed
- `team` - Group the actor belongs to
- `contextActivities` - Related activities (parent, grouping, category, other)
- `platform` - Platform where activity occurred
- `language` - Language code

### Statement Immutability

Statements are **immutable** once stored. The only way to "undo" a statement is to **void** it using the reserved verb `http://adlnet.gov/expapi/verbs/voided`.

## 🔌 API Endpoints

### Base URL

All xAPI endpoints are prefixed with `/xapi/`. The API service is accessible via Aspire service discovery at `https+http://apiservice`.

### Endpoints

#### `GET /xapi/about`

Returns information about the LRS, including supported xAPI versions.

**Response:**
```json
{
  "version": ["1.0.3"],
  "extensions": {
    "name": "xApiApp LRS",
    "description": "A .NET 10 implementation of an xAPI Learning Record Store"
  }
}
```

#### `POST /xapi/statements`

Stores one or more statements. Can accept a single statement object or an array of statements.

**Request Headers:**
```
X-Experience-API-Version: 1.0.3
Content-Type: application/json
```

**Request Body (Single Statement):**
```json
{
  "actor": { ... },
  "verb": { ... },
  "object": { ... }
}
```

**Request Body (Batch):**
```json
[
  { "actor": { ... }, "verb": { ... }, "object": { ... } },
  { "actor": { ... }, "verb": { ... }, "object": { ... } }
]
```

**Response:** Array of statement IDs (UUIDs)

#### `PUT /xapi/statements?statementId={id}`

Stores a single statement with a specific ID.

**Query Parameters:**
- `statementId` (required) - UUID for the statement

**Response:** `204 No Content`

#### `GET /xapi/statements`

Retrieves statements with optional filtering.

**Query Parameters:**
- `statementId` - Get a single statement by ID
- `voidedStatementId` - Get a voided statement by ID
- `agent` - Filter by agent (JSON object)
- `verb` - Filter by verb IRI
- `activity` - Filter by activity IRI
- `registration` - Filter by registration UUID
- `related_activities` - Include related activities in filter (boolean)
- `related_agents` - Include related agents in filter (boolean)
- `since` - Only return statements stored after this timestamp
- `until` - Only return statements stored before this timestamp
- `limit` - Maximum number of statements to return (default: 0 = no limit)
- `format` - Response format: "ids", "exact", or "canonical" (default: "exact")
- `attachments` - Include attachments (boolean, default: false)
- `ascending` - Return results in ascending order (boolean, default: false)

**Response:** `StatementResult` object or single `Statement`

#### `GET /health`

Health check endpoint for Aspire monitoring.

**Response:** `200 OK` with health status

## 🎨 Web Frontend

### Dashboard Features

The web frontend is a **Blazor Server** application that provides:

1. **Real-time Statement Monitoring**
   - Auto-refreshes every 5 seconds
   - Displays recent statements in reverse chronological order
   - Shows statement details: ID, actor, verb, object, results, timestamps

2. **Statistics Dashboard**
   - Total statements count
   - Unique actors count
   - Unique verbs count
   - Last update timestamp

3. **API Endpoints Display**
   - Lists all available xAPI endpoints
   - Shows HTTP methods and paths
   - Displays base URL with Aspire service discovery information

4. **Connection Status**
   - Visual indicator showing connection status
   - Pause/Resume auto-refresh functionality

### Technology

- **Blazor Server** - Server-side rendering with SignalR for real-time updates
- **Tailwind CSS** - Utility-first CSS framework for modern styling
- **shadcn-inspired Design** - Clean, modern UI components

### Frontend Architecture

```
xApiApp.Web/
├── Components/
│   ├── Pages/
│   │   └── Dashboard.razor      # Main dashboard component
│   └── Layout/
│       └── MainLayout.razor     # Application layout
├── Services/
│   └── XApiClient.cs            # HTTP client for API calls
└── Models/
    └── Statement.cs              # Frontend data models
```

The dashboard uses **server-side rendering** with **InteractiveServer** render mode, providing:
- Real-time updates without full page refreshes
- Server-side state management
- Automatic reconnection on connection loss

## 🔄 How Aspire Works in This Project

### Service Discovery

Aspire provides **automatic service discovery** between services:

```csharp
// In xApiApp.Web/Program.cs
builder.Services.AddHttpClient<XApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
});
```

The `https+http://apiservice` URL is resolved by Aspire to the actual API service endpoint. This allows:
- **Dynamic port assignment** - Services can use any available port
- **HTTPS preference** - Automatically uses HTTPS when available, falls back to HTTP
- **Service mesh integration** - Services communicate through Aspire's service mesh

### Service Configuration

In `xApiApp.AppHost/AppHost.cs`:

```csharp
var apiService = builder.AddProject<Projects.xApiApp_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.xApiApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);
```

This configuration:
- Registers the API service with health checks
- Configures the web frontend with external endpoints
- Creates a dependency relationship (web waits for API)
- Enables service discovery between services

### Benefits of Aspire

1. **Simplified Development** - No need to hardcode ports or URLs
2. **Centralized Monitoring** - All logs, metrics, and traces in one dashboard
3. **Health Checks** - Automatic health monitoring and recovery
4. **Service Discovery** - Automatic service location and communication
5. **Distributed Tracing** - End-to-end request tracing across services
6. **OpenTelemetry Integration** - Built-in observability

## 📝 Example Usage

### Posting a Statement via API

```bash
curl -X POST http://localhost:5532/xapi/statements \
  -H "X-Experience-API-Version: 1.0.3" \
  -H "Content-Type: application/json" \
  -d '{
    "actor": {
      "objectType": "Agent",
      "mbox": "mailto:learner@example.com",
      "name": "John Doe"
    },
    "verb": {
      "id": "http://adlnet.gov/expapi/verbs/completed",
      "display": {
        "en-US": "completed"
      }
    },
    "object": {
      "objectType": "Activity",
      "id": "http://example.com/xapi/activities/course1"
    },
    "result": {
      "success": true,
      "completion": true,
      "score": {
        "scaled": 0.95
      }
    }
  }'
```

### Retrieving Statements

```bash
curl -X GET "http://localhost:5532/xapi/statements?limit=10" \
  -H "X-Experience-API-Version: 1.0.3"
```

### Filtering by Agent

```bash
curl -X GET "http://localhost:5532/xapi/statements?agent=%7B%22objectType%22%3A%22Agent%22%2C%22mbox%22%3A%22mailto%3Alearner%40example.com%22%7D" \
  -H "X-Experience-API-Version: 1.0.3"
```

## 🧪 Testing

### Using the Dashboard

1. Start the application with `aspire run`
2. Navigate to the web dashboard
3. Use the API endpoints to post statements
4. Watch statements appear in real-time on the dashboard

### Example Test Statements

See `xApiApp.ApiService/xapi-examples.http` for example HTTP requests you can use to test the API.

## 📚 xAPI Specification

This implementation follows the [xAPI Specification 1.0.3](https://github.com/adlnet/xAPI-Spec). Key features:

- ✅ Statement storage and retrieval
- ✅ Statement immutability
- ✅ Statement voiding
- ✅ Query filtering
- ✅ Version header handling
- ✅ About endpoint
- ✅ Health checks

## 🔒 Security Considerations

Currently, the API does not implement authentication. For production use, you should:

1. **Add Authentication** - Implement OAuth 1.0 or HTTP Basic Authentication as per xAPI spec
2. **Add Authorization** - Implement scope-based permissions
3. **Use HTTPS** - Ensure all communications are encrypted
4. **Rate Limiting** - Implement rate limiting to prevent abuse
5. **Input Validation** - Enhanced validation of statement structure
6. **CORS Configuration** - Configure CORS appropriately for your use case

## 🗄️ Data Storage

**Current Implementation**: Statements are stored in-memory using `ConcurrentDictionary`. This means:
- Data is lost on service restart
- Not suitable for production use

**For Production**: Consider implementing persistence using:
- **Entity Framework Core** with SQL Server/PostgreSQL
- **MongoDB** for document storage
- **Azure Cosmos DB** for scalable cloud storage
- **Redis** for caching with a persistent backend

## 🚧 Future Enhancements

- [ ] Persistent storage (database integration)
- [ ] Authentication and authorization
- [ ] Statement attachments support
- [ ] Activity and Agent profile resources
- [ ] State resource implementation
- [ ] Advanced filtering and aggregation
- [ ] Export/import functionality
- [ ] Webhook support for real-time notifications
- [ ] GraphQL API option
- [ ] Multi-tenancy support

## 📄 License

This project is provided as-is for educational and development purposes.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📖 Resources

- [xAPI Specification](https://github.com/adlnet/xAPI-Spec)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)

## 🙏 Acknowledgments

- Built following the xAPI specification by ADL Initiative
- Uses .NET Aspire for distributed application orchestration
- Inspired by modern dashboard design patterns

---

**Built with ❤️ using .NET 10 and .NET Aspire**

