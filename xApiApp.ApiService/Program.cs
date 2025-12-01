using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using xApiApp.ApiService.Converters;
using xApiApp.ApiService.Middleware;
using xApiApp.ApiService.Models;
using xApiApp.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IStatementService, StatementService>();

// Configure JSON options for xAPI
// Note: xAPI uses exact property names (objectType, mbox_sha1sum, etc.), not camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null; // Use exact property names
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseXApiVersion();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Root endpoint
app.MapGet("/", () => "xAPI LRS API is running. Navigate to /xapi/about for LRS information.");

// xAPI About endpoint
app.MapGet("/xapi/about", () =>
{
    return new AboutResponse
    {
        Version = ["2.0.0"],
        Extensions = new Dictionary<string, object>
        {
            ["name"] = "xApiApp LRS",
            ["description"] = "A .NET 10 implementation of an xAPI Learning Record Store"
        }
    };
})
.WithName("GetAbout")
.Produces<AboutResponse>(200);

// xAPI Statements endpoints
var statementsGroup = app.MapGroup("/xapi/statements");

// POST Statements - Store single or batch
statementsGroup.MapPost("", async (HttpRequest request, IStatementService statementService) =>
{
    using var reader = new StreamReader(request.Body);
    var json = await reader.ReadToEndAsync();
    
    if (string.IsNullOrWhiteSpace(json))
    {
        return Results.BadRequest("Request body is required");
    }

    try
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null, // Use exact property names as defined in models
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());

        // Try to parse as array first (batch)
        if (json.TrimStart().StartsWith('['))
        {
            var statements = JsonSerializer.Deserialize<List<Statement>>(json, options);
            if (statements == null || statements.Count == 0)
            {
                return Results.BadRequest("Invalid statement batch");
            }

            ValidateStatements(statements);
            var ids = await statementService.StoreStatementsAsync(statements);
            return Results.Ok(ids);
        }
        else
        {
            // Single statement
            var statement = JsonSerializer.Deserialize<Statement>(json, options);
            if (statement == null)
            {
                return Results.BadRequest("Invalid statement");
            }

            ValidateStatement(statement);
            var id = await statementService.StoreStatementAsync(statement);
            return Results.Ok(new[] { id });
        }
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        // Log the full exception for debugging
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error processing statement: {Message}", ex.Message);
        return Results.Problem(
            detail: ex.ToString(),
            statusCode: 500);
    }
})
.WithName("PostStatements")
.Produces<List<string>>(200)
.Produces(400)
.Produces(409);

// PUT Statement - Store single statement with ID
statementsGroup.MapPut("", async (HttpRequest request, IStatementService statementService, string statementId) =>
{
    if (string.IsNullOrEmpty(statementId))
    {
        return Results.BadRequest("statementId parameter is required");
    }

    using var reader = new StreamReader(request.Body);
    var json = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(json))
    {
        return Results.BadRequest("Request body is required");
    }

    try
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null, // Use exact property names as defined in models
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var statement = JsonSerializer.Deserialize<Statement>(json, options);
        if (statement == null)
        {
            return Results.BadRequest("Invalid statement");
        }

        // Ensure statement ID matches parameter
        if (string.IsNullOrEmpty(statement.Id))
        {
            statement.Id = statementId;
        }
        else if (statement.Id != statementId)
        {
            return Results.BadRequest("Statement id must match statementId parameter");
        }

        ValidateStatement(statement);
        var id = await statementService.StoreStatementAsync(statement, statementId);
        
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        // Log the full exception for debugging
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error processing statement: {Message}", ex.Message);
        return Results.Problem(
            detail: ex.ToString(),
            statusCode: 500);
    }
})
.WithName("PutStatement")
.Produces(204)
.Produces(400)
.Produces(409);

// GET Statements - Retrieve statements with optional filters
statementsGroup.MapGet("", async (
    IStatementService statementService,
    string? statementId,
    string? voidedStatementId,
    string? agent,
    string? verb,
    string? activity,
    string? registration,
    bool? related_activities,
    bool? related_agents,
    string? since,
    string? until,
    int? limit,
    string? format,
    bool? attachments,
    bool? ascending) =>
{
    // Validate parameters
    if (!string.IsNullOrEmpty(statementId) && !string.IsNullOrEmpty(voidedStatementId))
    {
        return Results.BadRequest("Cannot specify both statementId and voidedStatementId");
    }

    if ((!string.IsNullOrEmpty(statementId) || !string.IsNullOrEmpty(voidedStatementId)) &&
        (!string.IsNullOrEmpty(agent) || !string.IsNullOrEmpty(verb) || !string.IsNullOrEmpty(activity) ||
         !string.IsNullOrEmpty(registration) || related_activities.HasValue || related_agents.HasValue ||
         !string.IsNullOrEmpty(since) || !string.IsNullOrEmpty(until) || limit.HasValue || ascending.HasValue))
    {
        return Results.BadRequest("Cannot specify other parameters with statementId or voidedStatementId");
    }

    try
    {
        var query = new StatementQuery
        {
            StatementId = statementId,
            VoidedStatementId = voidedStatementId,
            Verb = verb,
            Activity = activity,
            Registration = registration,
            RelatedActivities = related_activities ?? false,
            RelatedAgents = related_agents ?? false,
            Limit = limit ?? 0,
            Format = format ?? "exact",
            Attachments = attachments ?? false,
            Ascending = ascending ?? false
        };

        // Parse agent if provided
        if (!string.IsNullOrEmpty(agent))
        {
            try
            {
                var agentOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null // Use exact property names
                };
                agentOptions.Converters.Add(new JsonStringEnumConverter());
                
                query.Agent = JsonSerializer.Deserialize<Actor>(agent, agentOptions);
            }
            catch
            {
                return Results.BadRequest("Invalid agent parameter");
            }
        }

        // Parse timestamps
        if (!string.IsNullOrEmpty(since))
        {
            if (DateTimeOffset.TryParse(since, out var sinceDate))
            {
                query.Since = sinceDate;
            }
            else
            {
                return Results.BadRequest("Invalid since parameter");
            }
        }

        if (!string.IsNullOrEmpty(until))
        {
            if (DateTimeOffset.TryParse(until, out var untilDate))
            {
                query.Until = untilDate;
            }
            else
            {
                return Results.BadRequest("Invalid until parameter");
            }
        }

        var result = await statementService.GetStatementsAsync(query);

        // Single statement response
        if (!string.IsNullOrEmpty(statementId) || !string.IsNullOrEmpty(voidedStatementId))
        {
            if (result.Statements.Count == 0)
            {
                return Results.NotFound();
            }
            return Results.Ok(result.Statements.First());
        }

        // StatementResult response
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        // Log the full exception for debugging
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error processing statement: {Message}", ex.Message);
        return Results.Problem(
            detail: ex.ToString(),
            statusCode: 500);
    }
})
.WithName("GetStatements")
.Produces<Statement>(200)
.Produces<StatementResult>(200)
.Produces(400)
.Produces(404);

// Validation helpers
void ValidateStatement(Statement statement)
{
    if (statement.Actor == null)
    {
        throw new ArgumentException("Statement must include an actor");
    }

    if (statement.Verb == null)
    {
        throw new ArgumentException("Statement must include a verb");
    }

    if (string.IsNullOrEmpty(statement.Verb.Id))
    {
        throw new ArgumentException("Verb must include an id");
    }

    if (statement.Object == null)
    {
        throw new ArgumentException("Statement must include an object");
    }
}

void ValidateStatements(List<Statement> statements)
{
    foreach (var statement in statements)
    {
        ValidateStatement(statement);
    }
}

// Health check endpoint for Aspire
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .ExcludeFromDescription();

app.MapDefaultEndpoints();

app.Run();
