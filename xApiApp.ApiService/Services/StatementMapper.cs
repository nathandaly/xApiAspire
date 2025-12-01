using System.Text.Json;
using System.Text.Json.Serialization;
using xApiApp.ApiService.Data.Entities;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public class StatementMapper
{
    public static StatementEntity ToEntity(
        Statement statement,
        AgentEntity actor,
        VerbEntity verb,
        ActivityEntity? objectActivity,
        AgentEntity? objectAgent,
        StatementEntity? objectSubstatement,
        string? objectStatementRef,
        AgentEntity? authority,
        AgentEntity? contextInstructor,
        AgentEntity? contextTeam)
    {
        var entity = new StatementEntity
        {
            StatementId = statement.Id ?? Guid.NewGuid().ToString(),
            ActorId = actor.Id,
            VerbId = verb.Id,
            ObjectActivityId = objectActivity?.Id,
            ObjectAgentId = objectAgent?.Id,
            ObjectSubstatementId = objectSubstatement?.Id,
            ObjectStatementRef = objectStatementRef,
            Timestamp = statement.Timestamp ?? DateTimeOffset.UtcNow,
            Stored = statement.Stored ?? DateTimeOffset.UtcNow,
            Version = statement.Version ?? "2.0.0",
            AuthorityId = authority?.Id
        };

        // Store result as JSON
        if (statement.Result != null)
        {
            entity.Result = JsonSerializer.Serialize(statement.Result, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        // Store context
        if (statement.Context != null)
        {
            entity.ContextRegistration = statement.Context.Registration;
            entity.ContextInstructorId = contextInstructor?.Id;
            entity.ContextTeamId = contextTeam?.Id;
            entity.ContextRevision = statement.Context.Revision;
            entity.ContextPlatform = statement.Context.Platform;
            entity.ContextLanguage = statement.Context.Language;
            if (statement.Context.Statement != null)
            {
                entity.ContextStatement = statement.Context.Statement.Id;
            }
            if (statement.Context.Extensions != null)
            {
                entity.ContextExtensions = JsonSerializer.Serialize(statement.Context.Extensions);
            }
        }

        return entity;
    }

    public static Statement ToModel(
        StatementEntity entity,
        AgentEntity actor,
        VerbEntity verb,
        ActivityEntity? objectActivity,
        AgentEntity? objectAgent,
        StatementEntity? objectSubstatement,
        AgentEntity? contextInstructor,
        AgentEntity? contextTeam,
        AgentEntity? authority,
        List<ContextActivityEntity> contextActivities,
        List<StatementAttachmentEntity> attachments)
    {
        var statement = new Statement
        {
            Id = entity.StatementId,
            Timestamp = entity.Timestamp,
            Stored = entity.Stored,
            Version = entity.Version
        };

        // Load actor
        statement.Actor = ToActorModel(actor);

        // Load verb
        var verbData = JsonSerializer.Deserialize<Dictionary<string, object>>(verb.CanonicalData) ?? new Dictionary<string, object>();
        statement.Verb = new Verb
        {
            Id = verb.VerbId,
            Display = verbData.ContainsKey("display") && verbData["display"] is JsonElement displayElement
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(displayElement.GetRawText())
                : null
        };

        // Load object
        if (objectActivity != null)
        {
            var activityData = JsonSerializer.Deserialize<Dictionary<string, object>>(objectActivity.CanonicalData) ?? new Dictionary<string, object>();
            statement.Object = new Activity
            {
                Id = objectActivity.ActivityId,
                Definition = activityData.ContainsKey("definition") && activityData["definition"] is JsonElement defElement
                    ? JsonSerializer.Deserialize<ActivityDefinition>(defElement.GetRawText())
                    : null
            };
        }
        else if (objectAgent != null)
        {
            statement.Object = new AgentAsObject
            {
                Name = objectAgent.Name,
                Mbox = objectAgent.Mbox,
                MboxSha1Sum = objectAgent.MboxSha1Sum,
                OpenId = objectAgent.OpenId,
                Account = objectAgent.AccountHomePage != null && objectAgent.AccountName != null
                    ? new Account { HomePage = objectAgent.AccountHomePage, Name = objectAgent.AccountName }
                    : null
            };
        }
        else if (objectSubstatement != null)
        {
            // For substatements, we'll create a simplified version
            // Full recursion would require loading all related entities
            statement.Object = new SubStatement
            {
                Id = objectSubstatement.StatementId,
                Timestamp = objectSubstatement.Timestamp
            };
        }
        else if (!string.IsNullOrEmpty(entity.ObjectStatementRef))
        {
            statement.Object = new StatementRef { Id = entity.ObjectStatementRef };
        }

        // Load result
        if (!string.IsNullOrEmpty(entity.Result))
        {
            statement.Result = JsonSerializer.Deserialize<Result>(entity.Result);
        }

        // Load context
        if (entity.ContextRegistration != null || entity.ContextInstructorId.HasValue || 
            entity.ContextTeamId.HasValue || entity.ContextRevision != null ||
            entity.ContextPlatform != null || entity.ContextLanguage != null ||
            entity.ContextStatement != null || entity.ContextExtensions != null)
        {
            statement.Context = new Context
            {
                Registration = entity.ContextRegistration,
                Revision = entity.ContextRevision,
                Platform = entity.ContextPlatform,
                Language = entity.ContextLanguage
            };

            if (contextInstructor != null)
            {
                statement.Context.Instructor = ToActorModel(contextInstructor);
            }

            if (contextTeam != null)
            {
                statement.Context.Team = ToGroupModel(contextTeam, new List<AgentEntity>()); // Simplified - would need to load members
            }

            if (!string.IsNullOrEmpty(entity.ContextStatement))
            {
                statement.Context.Statement = new StatementRef { Id = entity.ContextStatement };
            }

            if (!string.IsNullOrEmpty(entity.ContextExtensions))
            {
                statement.Context.Extensions = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.ContextExtensions);
            }

            // Load context activities
            if (contextActivities.Any())
            {
                statement.Context.ContextActivities = new ContextActivities();
                foreach (var ca in contextActivities)
                {
                    var activity = new Activity { Id = ca.Activity.ActivityId };
                    
                    var list = ca.Type switch
                    {
                        "parent" => statement.Context.ContextActivities.Parent ??= new List<Activity>(),
                        "grouping" => statement.Context.ContextActivities.Grouping ??= new List<Activity>(),
                        "category" => statement.Context.ContextActivities.Category ??= new List<Activity>(),
                        "other" => statement.Context.ContextActivities.Other ??= new List<Activity>(),
                        _ => null
                    };
                    list?.Add(activity);
                }
            }
        }

        // Load authority
        if (authority != null)
        {
            statement.Authority = ToActorModel(authority);
        }

        // Load attachments
        if (attachments.Any())
        {
            statement.Attachments = attachments.Select(a =>
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(a.CanonicalData) ?? new Dictionary<string, object>();
                return new Attachment
                {
                    UsageType = data.ContainsKey("usageType") ? data["usageType"]?.ToString() : null,
                    ContentType = data.ContainsKey("contentType") ? data["contentType"]?.ToString() : null,
                    Length = data.ContainsKey("length") && data["length"] is JsonElement lenElement ? lenElement.GetInt64() : null,
                    Sha2 = data.ContainsKey("sha2") ? data["sha2"]?.ToString() : null,
                    FileUrl = data.ContainsKey("fileUrl") ? data["fileUrl"]?.ToString() : null
                };
            }).ToList();
        }

        return statement;
    }

    private static Actor ToActorModel(AgentEntity entity)
    {
        if (entity.ObjectType == "Group")
        {
            return new Group
            {
                Name = entity.Name,
                Mbox = entity.Mbox,
                MboxSha1Sum = entity.MboxSha1Sum,
                OpenId = entity.OpenId,
                Account = entity.AccountHomePage != null && entity.AccountName != null
                    ? new Account { HomePage = entity.AccountHomePage, Name = entity.AccountName }
                    : null
            };
        }

        return new Agent
        {
            Name = entity.Name,
            Mbox = entity.Mbox,
            MboxSha1Sum = entity.MboxSha1Sum,
            OpenId = entity.OpenId,
            Account = entity.AccountHomePage != null && entity.AccountName != null
                ? new Account { HomePage = entity.AccountHomePage, Name = entity.AccountName }
                : null
        };
    }

    private static Group ToGroupModel(AgentEntity entity, List<AgentEntity> members)
    {
        var group = new Group
        {
            Name = entity.Name,
            Mbox = entity.Mbox,
            MboxSha1Sum = entity.MboxSha1Sum,
            OpenId = entity.OpenId,
            Account = entity.AccountHomePage != null && entity.AccountName != null
                ? new Account { HomePage = entity.AccountHomePage, Name = entity.AccountName }
                : null
        };

        // Load members
        group.Member = members.Select(m => (Agent)ToActorModel(m)).ToList();
        return group;
    }
}

