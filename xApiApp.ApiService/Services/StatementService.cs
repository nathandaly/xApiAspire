using System.Collections.Concurrent;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public class StatementService : IStatementService
{
    private readonly ConcurrentDictionary<string, Statement> _statements = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _voidedStatements = new();

    public Task<string> StoreStatementAsync(Statement statement, string? statementId = null)
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

        // Set stored timestamp if not provided
        if (!statement.Stored.HasValue)
        {
            statement.Stored = DateTimeOffset.UtcNow;
        }

        // Set timestamp if not provided
        if (!statement.Timestamp.HasValue)
        {
            statement.Timestamp = DateTimeOffset.UtcNow;
        }

        // Check if statement already exists
        if (_statements.ContainsKey(statement.Id))
        {
            // Verify it matches (for immutability)
            var existing = _statements[statement.Id];
            if (!StatementsMatch(existing, statement))
            {
                throw new InvalidOperationException("Statement with this id already exists with different content");
            }
            return Task.FromResult(statement.Id);
        }

        // Store the statement
        _statements[statement.Id] = statement;

        // Check if this is a voiding statement
        if (statement.Verb?.Id == "http://adlnet.gov/expapi/verbs/voided" &&
            statement.Object is StatementRef statementRef)
        {
            if (!string.IsNullOrEmpty(statementRef.Id))
            {
                _voidedStatements.TryAdd(statementRef.Id, new HashSet<string>());
                _voidedStatements[statementRef.Id].Add(statement.Id);
            }
        }

        return Task.FromResult(statement.Id);
    }

    public Task<List<string>> StoreStatementsAsync(List<Statement> statements)
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
            var id = StoreStatementAsync(statement).Result;
            ids.Add(id);
        }

        return Task.FromResult(ids);
    }

    public Task<Statement?> GetStatementAsync(string statementId)
    {
        if (_statements.TryGetValue(statementId, out var statement))
        {
            // Check if voided
            if (IsVoided(statementId))
            {
                return Task.FromResult<Statement?>(null);
            }
            return Task.FromResult<Statement?>(statement);
        }
        return Task.FromResult<Statement?>(null);
    }

    public Task<StatementResult> GetStatementsAsync(StatementQuery query)
    {
        var result = new StatementResult();

        // Single statement by ID
        if (!string.IsNullOrEmpty(query.StatementId))
        {
            var statement = GetStatementAsync(query.StatementId).Result;
            if (statement != null)
            {
                result.Statements.Add(statement);
            }
            return Task.FromResult(result);
        }

        // Voided statement by ID
        if (!string.IsNullOrEmpty(query.VoidedStatementId))
        {
            if (_statements.TryGetValue(query.VoidedStatementId, out var statement))
            {
                result.Statements.Add(statement);
            }
            return Task.FromResult(result);
        }

        // Filter statements
        var filtered = _statements.Values
            .Where(s => !IsVoided(s.Id!))
            .AsEnumerable();

        // Apply filters
        if (query.Agent != null)
        {
            filtered = filtered.Where(s => MatchesAgent(s, query.Agent, query.RelatedAgents));
        }

        if (!string.IsNullOrEmpty(query.Verb))
        {
            filtered = filtered.Where(s => s.Verb?.Id == query.Verb);
        }

        if (!string.IsNullOrEmpty(query.Activity))
        {
            filtered = filtered.Where(s => MatchesActivity(s, query.Activity, query.RelatedActivities));
        }

        if (!string.IsNullOrEmpty(query.Registration))
        {
            filtered = filtered.Where(s => s.Context?.Registration == query.Registration);
        }

        if (query.Since.HasValue)
        {
            filtered = filtered.Where(s => s.Stored > query.Since);
        }

        if (query.Until.HasValue)
        {
            filtered = filtered.Where(s => s.Stored <= query.Until);
        }

        // Order by stored time
        if (query.Ascending)
        {
            filtered = filtered.OrderBy(s => s.Stored ?? DateTimeOffset.MinValue);
        }
        else
        {
            filtered = filtered.OrderByDescending(s => s.Stored ?? DateTimeOffset.MinValue);
        }

        // Apply limit
        if (query.Limit > 0)
        {
            filtered = filtered.Take(query.Limit);
        }

        result.Statements = filtered.ToList();
        return Task.FromResult(result);
    }

    public Task<bool> StatementExistsAsync(string statementId)
    {
        return Task.FromResult(_statements.ContainsKey(statementId));
    }

    private bool IsVoided(string statementId)
    {
        return _voidedStatements.ContainsKey(statementId);
    }

    private bool MatchesAgent(Statement statement, Actor agent, bool relatedAgents)
    {
        if (statement.Actor != null && AgentsMatch(statement.Actor, agent))
        {
            return true;
        }

        if (relatedAgents)
        {
            // Check if Object is an Agent or Group
            if (statement.Object != null)
            {
                if (statement.Object is AgentAsObject agentAsObject)
                {
                    var objAgent = new Agent
                    {
                        Name = agentAsObject.Name,
                        Mbox = agentAsObject.Mbox,
                        MboxSha1Sum = agentAsObject.MboxSha1Sum,
                        OpenId = agentAsObject.OpenId,
                        Account = agentAsObject.Account
                    };
                    if (AgentsMatch(objAgent, agent))
                    {
                        return true;
                    }
                }
                else if (statement.Object is GroupAsObject groupAsObject)
                {
                    var objGroup = new Group
                    {
                        Name = groupAsObject.Name,
                        Member = groupAsObject.Member,
                        Mbox = groupAsObject.Mbox,
                        MboxSha1Sum = groupAsObject.MboxSha1Sum,
                        OpenId = groupAsObject.OpenId,
                        Account = groupAsObject.Account
                    };
                    if (GroupsMatch(objGroup, agent))
                    {
                        return true;
                    }
                }
            }

            if (statement.Authority != null && AgentsMatch(statement.Authority, agent))
            {
                return true;
            }

            if (statement.Context != null)
            {
                if (statement.Context.Instructor != null && AgentsMatch(statement.Context.Instructor, agent))
                {
                    return true;
                }

                if (statement.Context.Team != null && GroupsMatch(statement.Context.Team, agent))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool MatchesActivity(Statement statement, string activityId, bool relatedActivities)
    {
        if (statement.Object is Activity activity && activity.Id == activityId)
        {
            return true;
        }

        if (relatedActivities && statement.Context?.ContextActivities != null)
        {
            var contextActivities = statement.Context.ContextActivities;
            if (contextActivities.Parent?.Any(a => a.Id == activityId) == true ||
                contextActivities.Grouping?.Any(a => a.Id == activityId) == true ||
                contextActivities.Category?.Any(a => a.Id == activityId) == true ||
                contextActivities.Other?.Any(a => a.Id == activityId) == true)
            {
                return true;
            }
        }

        return false;
    }

    private bool AgentsMatch(Actor a1, Actor a2)
    {
        if (a1 is Agent agent1 && a2 is Agent agent2)
        {
            return GetInverseFunctionalIdentifier(agent1) == GetInverseFunctionalIdentifier(agent2);
        }
        return false;
    }

    private bool GroupsMatch(Group group, Actor agent)
    {
        if (group.Member != null)
        {
            return group.Member.Any(m => AgentsMatch(m, agent));
        }
        return false;
    }

    private string? GetInverseFunctionalIdentifier(Agent agent)
    {
        if (!string.IsNullOrEmpty(agent.Mbox)) return $"mbox:{agent.Mbox}";
        if (!string.IsNullOrEmpty(agent.MboxSha1Sum)) return $"mbox_sha1sum:{agent.MboxSha1Sum}";
        if (!string.IsNullOrEmpty(agent.OpenId)) return $"openid:{agent.OpenId}";
        if (agent.Account != null) return $"account:{agent.Account.HomePage}:{agent.Account.Name}";
        return null;
    }

    private bool StatementsMatch(Statement s1, Statement s2)
    {
        // Simple comparison - in production, this should be more thorough
        // comparing all properties except id, stored, timestamp, authority
        return s1.Actor?.GetType() == s2.Actor?.GetType() &&
               s1.Verb?.Id == s2.Verb?.Id &&
               s1.Object?.GetType() == s2.Object?.GetType();
    }
}

