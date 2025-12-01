using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using xApiApp.ApiService.Data;
using xApiApp.ApiService.Data.Entities;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public interface IVerbService
{
    Task<VerbEntity> RetrieveOrCreateAsync(Verb verb);
}

public class VerbService : IVerbService
{
    private readonly XApiDbContext _context;

    public VerbService(XApiDbContext context)
    {
        _context = context;
    }

    public async Task<VerbEntity> RetrieveOrCreateAsync(Verb verb)
    {
        if (string.IsNullOrEmpty(verb.Id))
        {
            throw new ArgumentException("Verb ID is required");
        }

        var existing = await _context.Verbs.FirstOrDefaultAsync(v => v.VerbId == verb.Id);
        if (existing != null)
        {
            // Update canonical data if verb display changed
            var canonicalData = JsonSerializer.Serialize(new
            {
                id = verb.Id,
                display = verb.Display ?? new Dictionary<string, string>()
            });
            
            if (existing.CanonicalData != canonicalData)
            {
                existing.CanonicalData = canonicalData;
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        // Create new verb
        var verbEntity = new VerbEntity
        {
            VerbId = verb.Id,
            CanonicalData = JsonSerializer.Serialize(new
            {
                id = verb.Id,
                display = verb.Display ?? new Dictionary<string, string>()
            })
        };

        _context.Verbs.Add(verbEntity);
        await _context.SaveChangesAsync();
        return verbEntity;
    }
}

