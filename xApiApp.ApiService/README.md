# xAPI Learning Record Store (LRS) API

A .NET 10 implementation of an xAPI (Experience API) Learning Record Store based on the [xAPI Specification 2.0](https://opensource.ieee.org/xapi/xapi-base-standard-documentation).

## Features

- ✅ POST Statements (single or batch)
- ✅ PUT Statements (with statement ID)
- ✅ GET Statements (with filtering and query parameters)
- ✅ Statement immutability enforcement
- ✅ Statement voiding support
- ✅ X-Experience-API-Version header handling
- ✅ About endpoint for LRS information

## Endpoints

### About
- `GET /xapi/about` - Returns LRS information including supported xAPI versions

### Statements
- `POST /xapi/statements` - Store a single statement or batch of statements
- `PUT /xapi/statements?statementId={id}` - Store a single statement with a specific ID
- `GET /xapi/statements` - Retrieve statements with optional filters

## Request Headers

All requests (except `/xapi/about`) must include:
- `X-Experience-API-Version: 2.0.0` (or any 2.0.x version)
- `Content-Type: application/json` (for POST/PUT requests)

## Example Usage

### Store a Statement

```http
POST /xapi/statements
X-Experience-API-Version: 2.0.0
Content-Type: application/json

{
  "actor": {
    "objectType": "Agent",
    "mbox": "mailto:test@example.com",
    "name": "Test User"
  },
  "verb": {
    "id": "http://adlnet.gov/expapi/verbs/experienced",
    "display": {
      "en-US": "experienced"
    }
  },
  "object": {
    "objectType": "Activity",
    "id": "http://example.com/xapi/activities/myactivity"
  }
}
```

### Retrieve Statements

```http
GET /xapi/statements?limit=10&ascending=false
X-Experience-API-Version: 2.0.0
```

### Filter by Agent

```http
GET /xapi/statements?agent={"objectType":"Agent","mbox":"mailto:test@example.com"}
X-Experience-API-Version: 2.0.0
```

## Query Parameters

- `statementId` - Get a single statement by ID
- `voidedStatementId` - Get a voided statement by ID
- `agent` - Filter by agent (JSON object)
- `verb` - Filter by verb IRI
- `activity` - Filter by activity IRI
- `registration` - Filter by registration UUID
- `related_activities` - Include related activities in filter
- `related_agents` - Include related agents in filter
- `since` - Only return statements stored after this timestamp
- `until` - Only return statements stored before this timestamp
- `limit` - Maximum number of statements to return (0 = no limit)
- `format` - Response format: "ids", "exact", or "canonical" (default: "exact")
- `attachments` - Include attachments (default: false)
- `ascending` - Return results in ascending order (default: false)

## Statement Structure

A statement must include:
- `actor` - Agent or Group who performed the action
- `verb` - The action performed (with IRI and display)
- `object` - Activity, Agent, StatementRef, or SubStatement

Optional properties:
- `id` - UUID (generated if not provided)
- `result` - Result object with score, success, completion, etc.
- `context` - Context providing additional meaning
- `timestamp` - When the event occurred (set to current time if not provided)
- `stored` - When the statement was stored (set by LRS)
- `authority` - Who is asserting this statement (set by LRS)
- `version` - xAPI version (defaults to 2.0.0)
- `attachments` - Array of attachment objects

## Implementation Notes

- Statements are stored in-memory (not persisted to a database)
- Statement IDs are generated as UUIDs if not provided
- Statements are immutable once stored
- Voided statements are excluded from queries unless specifically requested
- The LRS validates statement structure and rejects invalid statements

## Running the API

This is an **Aspire application**. To run the full application stack:

```bash
dotnet aspire run
```

This will start the AppHost which orchestrates all services including the xAPI API service. The API will be available through the Aspire dashboard and at the configured endpoint.

Alternatively, to run just the API service:

```bash
cd xApiApp.ApiService
dotnet run
```

The API will be available at `http://localhost:5532` (HTTP) or `https://localhost:7395` (HTTPS) in development mode.

### Aspire Dashboard

When running with `dotnet aspire run`, you can access the Aspire dashboard which provides:
- Service discovery and endpoints
- Health check status
- Logs and metrics
- Service-to-service communication visualization

## Testing

See `xapi-examples.http` for example HTTP requests you can use to test the API.

