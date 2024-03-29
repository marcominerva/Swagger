using AwesomeBackend.BusinessLayer.Services.Common;
using AwesomeBackend.Common.Models.Responses;
using AwesomeBackend.DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Entities = AwesomeBackend.DataAccessLayer.Models;

namespace AwesomeBackend.BusinessLayer.Services;

public class RatingsService : BaseService, IRatingsService
{
    public RatingsService(IApplicationDbContext dataContext, IHttpContextAccessor httpContextAccessor, ILogger<RatingsService> logger, IServiceProvider serviceProvider)
        : base(dataContext, httpContextAccessor, logger, serviceProvider)
    {
    }

    public async Task<ListResult<Rating>> GetAsync(Guid restaurantId, int pageIndex, int itemsPerPage)
    {
        Logger.LogDebug("Trying to retrieve {ItemsCount} ratings for restaurants with Id {RestaurantId}...", itemsPerPage, restaurantId);

        var query = DataContext.GetData<Entities.Rating>().Where(r => r.RestaurantId == restaurantId);
        var totalCount = await query.LongCountAsync();

        Logger.LogDebug("Found {ItemsCounts} ratings", totalCount);

        var data = await query.Include(r => r.User)
            .OrderByDescending(s => s.Date)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)      // Try to retrieve an element more than the requested number to check whether there are more data.
            .Select(dbRating => CreateRatingDto(dbRating)).ToListAsync();

        var result = new ListResult<Rating>(data.Take(itemsPerPage), totalCount, data.Count > itemsPerPage);
        return result;
    }

    public async Task<Rating> GetAsync(Guid restaurantId, Guid id)
    {
        var dbRating = await DataContext.GetData<Entities.Rating>()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurantId);

        if (dbRating == null)
        {
            Logger.LogInformation("Unable to find rating with Id {Id} for Restaurant with Id {RestaurantId}", id, restaurantId);
            return null;
        }

        var rating = CreateRatingDto(dbRating);
        return rating;
    }

    public async Task<NewRating> RateAsync(Guid restaurantId, double score, string comment)
    {
        // Saves the new rating to the database.
        var dbRating = new Entities.Rating
        {
            RestaurantId = restaurantId,
            Score = score,
            Comment = comment,
            Date = DateTime.UtcNow,
            UserId = UserId.Value
        };

        DataContext.Insert(dbRating);
        await DataContext.SaveAsync();

        // Retrieves the new average rating for the restaurant.
        var averageScore = await DataContext.GetData<Entities.Rating>().Where(r => r.RestaurantId == restaurantId).AverageAsync(r => r.Score);
        var result = new NewRating(restaurantId, Math.Round(averageScore, 2));
        return result;
    }

    private static Rating CreateRatingDto(Entities.Rating dbRating)
        => new Rating
        {
            Id = dbRating.Id,
            RatingScore = dbRating.Score,
            Comment = dbRating.Comment,
            Date = dbRating.Date,
            User = $"{dbRating.User.FirstName} {dbRating.User.LastName}".Trim()
        };
}
