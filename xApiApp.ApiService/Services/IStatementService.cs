using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public interface IStatementService
{
    Task<string> StoreStatementAsync(Statement statement, string? statementId = null);
    Task<List<string>> StoreStatementsAsync(List<Statement> statements);
    Task<Statement?> GetStatementAsync(string statementId);
    Task<StatementResult> GetStatementsAsync(StatementQuery query);
    Task<bool> StatementExistsAsync(string statementId);
}

public class StatementQuery
{
    public string? StatementId { get; set; }
    public string? VoidedStatementId { get; set; }
    public Actor? Agent { get; set; }
    public string? Verb { get; set; }
    public string? Activity { get; set; }
    public string? Registration { get; set; }
    public bool RelatedActivities { get; set; }
    public bool RelatedAgents { get; set; }
    public DateTimeOffset? Since { get; set; }
    public DateTimeOffset? Until { get; set; }
    public int Limit { get; set; } = 0;
    public string Format { get; set; } = "exact";
    public bool Attachments { get; set; } = false;
    public bool Ascending { get; set; } = false;
}

