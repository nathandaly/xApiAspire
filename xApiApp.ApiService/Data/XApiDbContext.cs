using Microsoft.EntityFrameworkCore;
using xApiApp.ApiService.Data.Entities;

namespace xApiApp.ApiService.Data;

public class XApiDbContext : DbContext
{
    public XApiDbContext(DbContextOptions<XApiDbContext> options) : base(options)
    {
    }

    public DbSet<VerbEntity> Verbs { get; set; }
    public DbSet<AgentEntity> Agents { get; set; }
    public DbSet<AgentMemberEntity> AgentMembers { get; set; }
    public DbSet<ActivityEntity> Activities { get; set; }
    public DbSet<StatementEntity> Statements { get; set; }
    public DbSet<ContextActivityEntity> ContextActivities { get; set; }
    public DbSet<StatementAttachmentEntity> StatementAttachments { get; set; }
    public DbSet<ActivityStateEntity> ActivityStates { get; set; }
    public DbSet<ActivityProfileEntity> ActivityProfiles { get; set; }
    public DbSet<AgentProfileEntity> AgentProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Agent unique constraints
        modelBuilder.Entity<AgentEntity>()
            .HasIndex(a => a.Mbox)
            .IsUnique()
            .HasFilter("[mbox] IS NOT NULL");

        modelBuilder.Entity<AgentEntity>()
            .HasIndex(a => a.MboxSha1Sum)
            .IsUnique()
            .HasFilter("[mbox_sha1sum] IS NOT NULL");

        modelBuilder.Entity<AgentEntity>()
            .HasIndex(a => a.OpenId)
            .IsUnique()
            .HasFilter("[openid] IS NOT NULL");

        modelBuilder.Entity<AgentEntity>()
            .HasIndex(a => a.OAuthIdentifier)
            .IsUnique()
            .HasFilter("[oauth_identifier] IS NOT NULL");

        modelBuilder.Entity<AgentEntity>()
            .HasIndex(a => new { a.AccountHomePage, a.AccountName })
            .IsUnique()
            .HasFilter("[account_home_page] IS NOT NULL AND [account_name] IS NOT NULL");

        // Configure Agent Member relationships
        modelBuilder.Entity<AgentMemberEntity>()
            .HasOne(am => am.Group)
            .WithMany(a => a.GroupMemberships)
            .HasForeignKey(am => am.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AgentMemberEntity>()
            .HasOne(am => am.Member)
            .WithMany(a => a.MemberOfGroups)
            .HasForeignKey(am => am.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Statement relationships
        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.Actor)
            .WithMany()
            .HasForeignKey(s => s.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.Verb)
            .WithMany()
            .HasForeignKey(s => s.VerbId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.ObjectActivity)
            .WithMany()
            .HasForeignKey(s => s.ObjectActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.ObjectAgent)
            .WithMany()
            .HasForeignKey(s => s.ObjectAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.ObjectSubstatement)
            .WithMany()
            .HasForeignKey(s => s.ObjectSubstatementId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.ContextInstructor)
            .WithMany()
            .HasForeignKey(s => s.ContextInstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.ContextTeam)
            .WithMany()
            .HasForeignKey(s => s.ContextTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatementEntity>()
            .HasOne(s => s.Authority)
            .WithMany()
            .HasForeignKey(s => s.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Context Activities
        modelBuilder.Entity<ContextActivityEntity>()
            .HasOne(ca => ca.Statement)
            .WithMany(s => s.ContextActivities)
            .HasForeignKey(ca => ca.StatementId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ContextActivityEntity>()
            .HasOne(ca => ca.Activity)
            .WithMany()
            .HasForeignKey(ca => ca.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Attachments
        modelBuilder.Entity<StatementAttachmentEntity>()
            .HasOne(sa => sa.Statement)
            .WithMany(s => s.Attachments)
            .HasForeignKey(sa => sa.StatementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Activity State
        modelBuilder.Entity<ActivityStateEntity>()
            .HasOne(as_ => as_.Agent)
            .WithMany()
            .HasForeignKey(as_ => as_.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Agent Profile
        modelBuilder.Entity<AgentProfileEntity>()
            .HasOne(ap => ap.Agent)
            .WithMany()
            .HasForeignKey(ap => ap.AgentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

