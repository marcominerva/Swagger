﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AwesomeBackend.Authentication.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    private string email;
    public override string Email
    {
        get => email ?? userName;
        set => email = value;
    }

    private string userName;
    public override string UserName
    {
        get => userName ?? email;
        set => userName = value;
    }

    [Required]
    [StringLength(256)]
    public string FirstName { get; set; }

    [StringLength(256)]
    public string LastName { get; set; }
}
