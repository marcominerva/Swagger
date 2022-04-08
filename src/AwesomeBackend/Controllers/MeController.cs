using AwesomeBackend.Authentication.Extensions;
using AwesomeBackend.Common.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AwesomeBackend.Controllers;

[Authorize]
public class MeController : ControllerBase
{
    /// <summary>
    /// Return information about the currently logged user
    /// </summary>
    [HttpGet]
    [ProducesDefaultResponseType]
    public ActionResult<User> Get()
    {
        // Get User information from claims
        return new User
        {
            Id = User.GetId(),
            UserName = User.GetUserName(),
            FirstName = User.GetFirstName(),
            LastName = User.GetLastName(),
            Email = User.GetEmail()
        };
    }
}
