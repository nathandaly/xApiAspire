using Microsoft.EntityFrameworkCore;
using xApiApp.ApiService.Data;
using xApiApp.ApiService.Data.Entities;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public interface IAgentService
{
    Task<AgentEntity> RetrieveOrCreateAsync(Actor actor);
    Task<AgentEntity?> RetrieveAsync(Actor actor);
}

public class AgentService : IAgentService
{
    private readonly XApiDbContext _context;

    public AgentService(XApiDbContext context)
    {
        _context = context;
    }

    public async Task<AgentEntity?> RetrieveAsync(Actor actor)
    {
        // Check IFIs in order: mbox, mbox_sha1sum, openid, account
        if (actor is Agent agent)
        {
            if (!string.IsNullOrEmpty(agent.Mbox))
            {
                return await _context.Agents.FirstOrDefaultAsync(a => a.Mbox == agent.Mbox);
            }
            if (!string.IsNullOrEmpty(agent.MboxSha1Sum))
            {
                return await _context.Agents.FirstOrDefaultAsync(a => a.MboxSha1Sum == agent.MboxSha1Sum);
            }
            if (!string.IsNullOrEmpty(agent.OpenId))
            {
                return await _context.Agents.FirstOrDefaultAsync(a => a.OpenId == agent.OpenId);
            }
            if (agent.Account != null && !string.IsNullOrEmpty(agent.Account.HomePage) && !string.IsNullOrEmpty(agent.Account.Name))
            {
                return await _context.Agents.FirstOrDefaultAsync(a => 
                    a.AccountHomePage == agent.Account.HomePage && 
                    a.AccountName == agent.Account.Name);
            }
        }
        return null;
    }

    public async Task<AgentEntity> RetrieveOrCreateAsync(Actor actor)
    {
        var existing = await RetrieveAsync(actor);
        if (existing != null)
        {
            return existing;
        }

        // Create new agent
        var agentEntity = new AgentEntity
        {
            ObjectType = actor.ObjectType,
            Name = actor.Name
        };

        if (actor is Agent agent)
        {
            agentEntity.Mbox = agent.Mbox;
            agentEntity.MboxSha1Sum = agent.MboxSha1Sum;
            agentEntity.OpenId = agent.OpenId;
            if (agent.Account != null)
            {
                agentEntity.AccountHomePage = agent.Account.HomePage;
                agentEntity.AccountName = agent.Account.Name;
            }
        }
        else if (actor is Group group)
        {
            agentEntity.ObjectType = "Group";
            agentEntity.Mbox = group.Mbox;
            agentEntity.MboxSha1Sum = group.MboxSha1Sum;
            agentEntity.OpenId = group.OpenId;
            if (group.Account != null)
            {
                agentEntity.AccountHomePage = group.Account.HomePage;
                agentEntity.AccountName = group.Account.Name;
            }

            // Add members if provided
            if (group.Member != null && group.Member.Count > 0)
            {
                _context.Agents.Add(agentEntity);
                await _context.SaveChangesAsync();

                foreach (var member in group.Member)
                {
                    var memberEntity = await RetrieveOrCreateAsync(member);
                    _context.AgentMembers.Add(new AgentMemberEntity
                    {
                        GroupId = agentEntity.Id,
                        MemberId = memberEntity.Id
                    });
                }
            }
        }

        _context.Agents.Add(agentEntity);
        await _context.SaveChangesAsync();
        return agentEntity;
    }
}

