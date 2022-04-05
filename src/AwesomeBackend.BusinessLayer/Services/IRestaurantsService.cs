using AwesomeBackend.Common.Models.Responses;

namespace AwesomeBackend.BusinessLayer.Services;

public interface IRestaurantsService
{
    Task<Restaurant> GetAsync(Guid id);

    Task<ListResult<Restaurant>> GetAsync(int pageIndex, int itemsPerPage);
}
