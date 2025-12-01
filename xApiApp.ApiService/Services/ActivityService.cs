using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using xApiApp.ApiService.Data;
using xApiApp.ApiService.Data.Entities;
using xApiApp.ApiService.Models;

namespace xApiApp.ApiService.Services;

public interface IActivityService
{
    Task<ActivityEntity> RetrieveOrCreateAsync(Activity activity);
}

public class ActivityService : IActivityService
{
    private readonly XApiDbContext _context;

    public ActivityService(XApiDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityEntity> RetrieveOrCreateAsync(Activity activity)
    {
        if (string.IsNullOrEmpty(activity.Id))
        {
            throw new ArgumentException("Activity ID is required");
        }

        var existing = await _context.Activities.FirstOrDefaultAsync(a => a.ActivityId == activity.Id);
        if (existing != null)
        {
            // Update canonical data if activity definition changed
            var canonicalData = JsonSerializer.Serialize(new
            {
                id = activity.Id,
                definition = activity.Definition != null ? new
                {
                    name = activity.Definition.Name,
                    description = activity.Definition.Description,
                    type = activity.Definition.Type,
                    moreInfo = activity.Definition.MoreInfo,
                    interactionType = activity.Definition.InteractionType,
                    correctResponsesPattern = activity.Definition.CorrectResponsesPattern,
                    choices = activity.Definition.Choices,
                    scale = activity.Definition.Scale,
                    source = activity.Definition.Source,
                    target = activity.Definition.Target,
                    steps = activity.Definition.Steps,
                    extensions = activity.Definition.Extensions
                } : null
            }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

            if (existing.CanonicalData != canonicalData)
            {
                existing.CanonicalData = canonicalData;
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        // Create new activity
        var activityEntity = new ActivityEntity
        {
            ActivityId = activity.Id,
            CanonicalData = JsonSerializer.Serialize(new
            {
                id = activity.Id,
                definition = activity.Definition != null ? new
                {
                    name = activity.Definition.Name,
                    description = activity.Definition.Description,
                    type = activity.Definition.Type,
                    moreInfo = activity.Definition.MoreInfo,
                    interactionType = activity.Definition.InteractionType,
                    correctResponsesPattern = activity.Definition.CorrectResponsesPattern,
                    choices = activity.Definition.Choices,
                    scale = activity.Definition.Scale,
                    source = activity.Definition.Source,
                    target = activity.Definition.Target,
                    steps = activity.Definition.Steps,
                    extensions = activity.Definition.Extensions
                } : null
            }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull })
        };

        _context.Activities.Add(activityEntity);
        await _context.SaveChangesAsync();
        return activityEntity;
    }
}

