using Microsoft.EntityFrameworkCore;
using xApiApp.ApiService.Data;
using xApiApp.ApiService.Data.Entities;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public class StatementService : IStatementService
{
    private readonly XApiDbContext _context;
    private readonly IAgentService _agentService;
    private readonly IVerbService _verbService;
    private readonly IActivityService _activityService;

    public StatementService(
        XApiDbContext context,
        IAgentService agentService,
        IVerbService verbService,
        IActivityService activityService)
    {
        _context = context;
        _agentService = agentService;
        _verbService = verbService;
        _activityService = activityService;
    }

    public async Task<string> StoreStatementAsync(Statement statement, string? statementId = null)
    {
        // Generate ID if not provided
        if (string.IsNullOrEmpty(statement.Id))
        {
            statement.Id = Guid.NewGuid().ToString();
        }

        // Use provided statementId if given (for PUT requests)
        if (!string.IsNullOrEmpty(statementId))
        {
            if (statement.Id != statementId)
            {
                throw new ArgumentException("Statement id must match statementId parameter");
            }
        }

        // Check if statement already exists
        var existingEntity = await _context.Statements
            .FirstOrDefaultAsync(s => s.StatementId == statement.Id);

        if (existingEntity != null)
        {
            // Verify it matches (for immutability) - simplified check
            // In production, you'd want a more thorough comparison
            return statement.Id;
        }

        // Set timestamps if not provided
        var stored = statement.Stored ?? DateTimeOffset.UtcNow;
        var timestamp = statement.Timestamp ?? DateTimeOffset.UtcNow;

        // Retrieve or create Actor
        if (statement.Actor == null)
        {
            throw new ArgumentException("Statement must include an actor");
        }
        var actorEntity = await _agentService.RetrieveOrCreateAsync(statement.Actor);

        // Retrieve or create Verb
        if (statement.Verb == null)
        {
            throw new ArgumentException("Statement must include a verb");
        }
        var verbEntity = await _verbService.RetrieveOrCreateAsync(statement.Verb);

        // Retrieve or create Object
        ActivityEntity? objectActivity = null;
        AgentEntity? objectAgent = null;
        StatementEntity? objectSubstatement = null;
        string? objectStatementRef = null;

        if (statement.Object == null)
        {
            throw new ArgumentException("Statement must include an object");
        }

        switch (statement.Object)
        {
            case Activity activity:
                objectActivity = await _activityService.RetrieveOrCreateAsync(activity);
                break;
            case AgentAsObject agentAsObject:
                var agentActor = new Agent
                {
                    Name = agentAsObject.Name,
                    Mbox = agentAsObject.Mbox,
                    MboxSha1Sum = agentAsObject.MboxSha1Sum,
                    OpenId = agentAsObject.OpenId,
                    Account = agentAsObject.Account
                };
                objectAgent = await _agentService.RetrieveOrCreateAsync(agentActor);
                break;
            case GroupAsObject groupAsObject:
                var groupActor = new Group
                {
                    Name = groupAsObject.Name,
                    Mbox = groupAsObject.Mbox,
                    MboxSha1Sum = groupAsObject.MboxSha1Sum,
                    OpenId = groupAsObject.OpenId,
                    Account = groupAsObject.Account,
                    Member = groupAsObject.Member
                };
                objectAgent = await _agentService.RetrieveOrCreateAsync(groupActor);
                break;
            case StatementRef statementRef:
                objectStatementRef = statementRef.Id;
                break;
            case SubStatement subStatement:
                // Store substatement first recursively
                var subId = await StoreStatementAsync(new Statement
                {
                    Id = subStatement.Id ?? Guid.NewGuid().ToString(),
                    Actor = subStatement.Actor,
                    Verb = subStatement.Verb,
                    Object = subStatement.Object,
                    Result = subStatement.Result,
                    Context = subStatement.Context,
                    Timestamp = subStatement.Timestamp,
                    Attachments = subStatement.Attachments
                });
                objectSubstatement = await _context.Statements.FirstAsync(s => s.StatementId == subId);
                break;
        }

        // Retrieve or create Authority
        AgentEntity? authorityEntity = null;
        if (statement.Authority != null)
        {
            authorityEntity = await _agentService.RetrieveOrCreateAsync(statement.Authority);
        }

        // Retrieve or create Context entities
        AgentEntity? contextInstructor = null;
        AgentEntity? contextTeam = null;
        if (statement.Context != null)
        {
            if (statement.Context.Instructor != null)
            {
                contextInstructor = await _agentService.RetrieveOrCreateAsync(statement.Context.Instructor);
            }
            if (statement.Context.Team != null)
            {
                contextTeam = await _agentService.RetrieveOrCreateAsync(statement.Context.Team);
            }
        }

        // Create statement entity
        var statementEntity = StatementMapper.ToEntity(
            statement,
            actorEntity,
            verbEntity,
            objectActivity,
            objectAgent,
            objectSubstatement,
            objectStatementRef,
            authorityEntity,
            contextInstructor,
            contextTeam);

        statementEntity.Stored = stored;
        statementEntity.Timestamp = timestamp;

        _context.Statements.Add(statementEntity);
        await _context.SaveChangesAsync();

        // Store context activities
        if (statement.Context?.ContextActivities != null)
        {
            var contextActivityTypes = new[]
            {
                ("parent", statement.Context.ContextActivities.Parent),
                ("grouping", statement.Context.ContextActivities.Grouping),
                ("category", statement.Context.ContextActivities.Category),
                ("other", statement.Context.ContextActivities.Other)
            };

            foreach (var (type, activities) in contextActivityTypes)
            {
                if (activities != null)
                {
                    foreach (var activity in activities)
                    {
                        var activityEntity = await _activityService.RetrieveOrCreateAsync(activity);
                        _context.ContextActivities.Add(new ContextActivityEntity
                        {
                            StatementId = statementEntity.Id,
                            ActivityId = activityEntity.Id,
                            Type = type
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        // Check if this is a voiding statement
        if (statement.Verb.Id == "http://adlnet.gov/expapi/verbs/voided" &&
            statement.Object is StatementRef voidedRef)
        {
            // Voided statements are tracked by checking if a voiding statement exists
            // This is handled in the query logic
        }

        return statement.Id;
    }

    public async Task<List<string>> StoreStatementsAsync(List<Statement> statements)
    {
        var ids = new List<string>();
        var statementIds = new HashSet<string>();

        // Check for duplicate IDs in batch
        foreach (var statement in statements)
        {
            if (!string.IsNullOrEmpty(statement.Id))
            {
                if (statementIds.Contains(statement.Id))
                {
                    throw new ArgumentException("Batch contains duplicate statement IDs");
                }
                statementIds.Add(statement.Id);
            }
        }

        // Store each statement
        foreach (var statement in statements)
        {
            var id = await StoreStatementAsync(statement);
            ids.Add(id);
        }

        return ids;
    }

    public async Task<Statement?> GetStatementAsync(string statementId)
    {
        var entity = await _context.Statements
            .Include(s => s.Actor)
            .Include(s => s.Verb)
            .Include(s => s.ObjectActivity)
            .Include(s => s.ObjectAgent)
            .Include(s => s.ObjectSubstatement)
            .Include(s => s.ContextInstructor)
            .Include(s => s.ContextTeam)
            .Include(s => s.Authority)
            .Include(s => s.ContextActivities)
                .ThenInclude(ca => ca.Activity)
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.StatementId == statementId);

        if (entity == null)
        {
            return null;
        }

        // Check if voided
        var isVoided = await IsVoidedAsync(statementId);
        if (isVoided)
        {
            return null;
        }

        return StatementMapper.ToModel(
            entity,
            entity.Actor,
            entity.Verb,
            entity.ObjectActivity,
            entity.ObjectAgent,
            entity.ObjectSubstatement,
            entity.ContextInstructor,
            entity.ContextTeam,
            entity.Authority,
            entity.ContextActivities.ToList(),
            entity.Attachments.ToList());
    }

    public async Task<StatementResult> GetStatementsAsync(StatementQuery query)
    {
        var result = new StatementResult();

        // Single statement by ID
        if (!string.IsNullOrEmpty(query.StatementId))
        {
            var statement = await GetStatementAsync(query.StatementId);
            if (statement != null)
            {
                result.Statements.Add(statement);
            }
            return result;
        }

        // Voided statement by ID
        if (!string.IsNullOrEmpty(query.VoidedStatementId))
        {
            var entity = await _context.Statements
                .Include(s => s.Actor)
                .Include(s => s.Verb)
                .Include(s => s.ObjectActivity)
                .Include(s => s.ObjectAgent)
                .Include(s => s.ContextActivities)
                    .ThenInclude(ca => ca.Activity)
                .FirstOrDefaultAsync(s => s.StatementId == query.VoidedStatementId);

            if (entity != null)
            {
                result.Statements.Add(StatementMapper.ToModel(
                    entity,
                    entity.Actor,
                    entity.Verb,
                    entity.ObjectActivity,
                    entity.ObjectAgent,
                    entity.ObjectSubstatement,
                    entity.ContextInstructor,
                    entity.ContextTeam,
                    entity.Authority,
                    entity.ContextActivities.ToList(),
                    entity.Attachments.ToList()));
            }
            return result;
        }

        // Get all voided statement IDs
        var voidedIds = await GetVoidedStatementIdsAsync();

        // Build query
        var dbQuery = _context.Statements
            .Include(s => s.Actor)
            .Include(s => s.Verb)
            .Include(s => s.ObjectActivity)
            .Include(s => s.ObjectAgent)
            .Include(s => s.ObjectSubstatement)
            .Include(s => s.ContextInstructor)
            .Include(s => s.ContextTeam)
            .Include(s => s.Authority)
            .Include(s => s.ContextActivities)
                .ThenInclude(ca => ca.Activity)
            .Include(s => s.Attachments)
            .Where(s => !voidedIds.Contains(s.StatementId))
            .AsQueryable();

        // Apply filters
        if (query.Agent != null)
        {
            var agentEntity = await _agentService.RetrieveAsync(query.Agent);
            if (agentEntity != null)
            {
                if (query.RelatedAgents)
                {
                    dbQuery = dbQuery.Where(s =>
                        s.ActorId == agentEntity.Id ||
                        s.ObjectAgentId == agentEntity.Id ||
                        s.AuthorityId == agentEntity.Id ||
                        s.ContextInstructorId == agentEntity.Id ||
                        s.ContextTeamId == agentEntity.Id);
                }
                else
                {
                    dbQuery = dbQuery.Where(s => s.ActorId == agentEntity.Id);
                }
            }
            else
            {
                // Agent not found, return empty result
                return result;
            }
        }

        if (!string.IsNullOrEmpty(query.Verb))
        {
            var verbEntity = await _context.Verbs.FirstOrDefaultAsync(v => v.VerbId == query.Verb);
            if (verbEntity != null)
            {
                dbQuery = dbQuery.Where(s => s.VerbId == verbEntity.Id);
            }
            else
            {
                return result;
            }
        }

        if (!string.IsNullOrEmpty(query.Activity))
        {
            var activityEntity = await _context.Activities.FirstOrDefaultAsync(a => a.ActivityId == query.Activity);
            if (activityEntity != null)
            {
                if (query.RelatedActivities)
                {
                    dbQuery = dbQuery.Where(s =>
                        s.ObjectActivityId == activityEntity.Id ||
                        s.ContextActivities.Any(ca => ca.ActivityId == activityEntity.Id));
                }
                else
                {
                    dbQuery = dbQuery.Where(s => s.ObjectActivityId == activityEntity.Id);
                }
            }
            else
            {
                return result;
            }
        }

        if (!string.IsNullOrEmpty(query.Registration))
        {
            dbQuery = dbQuery.Where(s => s.ContextRegistration == query.Registration);
        }

        if (query.Since.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.Stored > query.Since.Value);
        }

        if (query.Until.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.Stored <= query.Until.Value);
        }

        // Fetch entities first (SQLite doesn't support DateTimeOffset in ORDER BY)
        var entities = await dbQuery.ToListAsync();

        // Order by stored time in memory
        if (query.Ascending)
        {
            entities = entities.OrderBy(s => s.Stored).ToList();
        }
        else
        {
            entities = entities.OrderByDescending(s => s.Stored).ToList();
        }

        // Apply limit
        if (query.Limit > 0)
        {
            entities = entities.Take(query.Limit).ToList();
        }

        // Convert to models
        foreach (var entity in entities)
        {
            result.Statements.Add(StatementMapper.ToModel(
                entity,
                entity.Actor,
                entity.Verb,
                entity.ObjectActivity,
                entity.ObjectAgent,
                entity.ObjectSubstatement,
                entity.ContextInstructor,
                entity.ContextTeam,
                entity.Authority,
                entity.ContextActivities.ToList(),
                entity.Attachments.ToList()));
        }

        return result;
    }

    public Task<bool> StatementExistsAsync(string statementId)
    {
        return _context.Statements.AnyAsync(s => s.StatementId == statementId);
    }

    private async Task<bool> IsVoidedAsync(string statementId)
    {
        // Check if there's a voiding statement that references this statement
        var voidingStatement = await _context.Statements
            .Include(s => s.Verb)
            .FirstOrDefaultAsync(s =>
                s.Verb.VerbId == "http://adlnet.gov/expapi/verbs/voided" &&
                s.ObjectStatementRef == statementId);

        return voidingStatement != null;
    }

    private async Task<HashSet<string>> GetVoidedStatementIdsAsync()
    {
        var voidingStatements = await _context.Statements
            .Include(s => s.Verb)
            .Where(s => s.Verb.VerbId == "http://adlnet.gov/expapi/verbs/voided" &&
                       s.ObjectStatementRef != null)
            .Select(s => s.ObjectStatementRef!)
            .ToListAsync();

        return new HashSet<string>(voidingStatements);
    }
}
