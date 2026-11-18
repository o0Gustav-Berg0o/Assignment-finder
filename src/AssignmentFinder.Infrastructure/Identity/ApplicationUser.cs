using Microsoft.AspNetCore.Identity;

namespace AssignmentFinder.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public const string AdminRole = "Admin";

    /// <summary>
    /// Self-registered users start unapproved and cannot log in until an admin approves them.
    /// The bootstrap admin (see AdminSeeder) is always created already approved.
    /// </summary>
    public bool IsApproved { get; set; }
}
