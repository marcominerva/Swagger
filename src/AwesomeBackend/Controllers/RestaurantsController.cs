using AwesomeBackend.BusinessLayer.Services;
using AwesomeBackend.Common.Models.Requests;
using AwesomeBackend.Common.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AwesomeBackend.Controllers
{
    public class RestaurantsController : ControllerBase
    {
        private readonly IRestaurantsService restaurantsService;
        private readonly IRatingsService ratingsService;

        public RestaurantsController(IRestaurantsService restaurantsService, IRatingsService ratingsService)
        {
            this.restaurantsService = restaurantsService;
            this.ratingsService = ratingsService;
        }

        /// <summary>
        /// Get the paginated restaurants list
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ListResult<Restaurant>>> GetRestaurantsList([FromQuery(Name = "page")] int pageIndex = 0,
                                                                                   [FromQuery(Name = "size")] int itemsPerPage = 20)
        {
            var restaurants = await restaurantsService.GetAsync(pageIndex, itemsPerPage);
            return restaurants;
        }

        /// <summary>
        /// Get a specific restaurant
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Restaurant), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Restaurant>> GetRestaurant(Guid id)
        {
            var restaurant = await restaurantsService.GetAsync(id);
            if (restaurant != null)
            {
                return restaurant;
            }

            return NotFound();
        }

        /// <summary>
        /// Get the paginated ratings of the given restaurant
        /// </summary>
        [HttpGet("{id:guid}/ratings")]
        public async Task<ActionResult<ListResult<Rating>>> GetRatingsList([FromRoute(Name = "id")] Guid restaurantId,
                                                                           [FromQuery(Name = "page")] int pageIndex = 0,
                                                                           [FromQuery(Name = "size")] int itemsPerPage = 20)
        {
            var ratings = await ratingsService.GetAsync(restaurantId, pageIndex, itemsPerPage);
            return ratings;
        }

        /// <summary>
        /// Get a specific rating
        /// </summary>
        [HttpGet("{id:guid}/ratings/{ratingId:guid}")]
        public async Task<ActionResult<Rating>> GetRating(Guid id, Guid ratingId)
        {
            var rating = await ratingsService.GetAsync(id, ratingId);
            if (rating != null)
            {
                return rating;
            }

            return NotFound();
        }

        /// <summary>
        /// Send a new rating for a restaurant
        /// </summary>
        [Authorize]
        [HttpPost("{id:guid}/ratings")]
        public async Task<ActionResult<NewRating>> Rate([FromRoute(Name = "id")] Guid restaurantId, RatingRequest rating)
        {
            var result = await ratingsService.RateAsync(restaurantId, rating.Score, rating.Comment);
            return result;
        }
    }
}
