﻿using AwesomeBackend.Common.Models.Responses;

namespace AwesomeBackend.BusinessLayer.Services;

public interface IRatingsService
{
    Task<ListResult<Rating>> GetAsync(Guid restaurantId, int pageIndex, int itemsPerPage);

    Task<Rating> GetAsync(Guid restaurantId, Guid id);

    Task<NewRating> RateAsync(Guid restaurantId, double score, string comment);
}
