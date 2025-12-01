using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace xApiApp.ApiService.Data.Entities;

[Table("agents")]
[Index(nameof(Mbox))]
[Index(nameof(MboxSha1Sum))]
[Index(nameof(OpenId))]
[Index(nameof(OAuthIdentifier))]
[Index(nameof(AccountHomePage), nameof(AccountName))]
public class AgentEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [MaxLength(6)]
    [Column("object_type")]
    public string ObjectType { get; set; } = "Agent";

    [MaxLength(100)]
    [Column("name")]
    public string? Name { get; set; }

    [MaxLength(128)]
    [Column("mbox")]
    public string? Mbox { get; set; }

    [MaxLength(40)]
    [Column("mbox_sha1sum")]
    public string? MboxSha1Sum { get; set; }

    [MaxLength(2048)]
    [Column("openid")]
    public string? OpenId { get; set; }

    [MaxLength(192)]
    [Column("oauth_identifier")]
    public string? OAuthIdentifier { get; set; }

    [MaxLength(2048)]
    [Column("account_home_page")]
    public string? AccountHomePage { get; set; }

    [MaxLength(50)]
    [Column("account_name")]
    public string? AccountName { get; set; }

    // Many-to-many relationship for Group members
    [InverseProperty(nameof(AgentMemberEntity.Group))]
    public virtual ICollection<AgentMemberEntity> GroupMemberships { get; set; } = new List<AgentMemberEntity>();

    [InverseProperty(nameof(AgentMemberEntity.Member))]
    public virtual ICollection<AgentMemberEntity> MemberOfGroups { get; set; } = new List<AgentMemberEntity>();
}

[Table("agent_members")]
public class AgentMemberEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("group_id")]
    public int GroupId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public virtual AgentEntity Group { get; set; } = null!;

    [ForeignKey(nameof(MemberId))]
    public virtual AgentEntity Member { get; set; } = null!;
}

