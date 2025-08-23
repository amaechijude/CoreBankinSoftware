// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using CustomerAPI.Entities;

// namespace CustomerProfile.API.Entities
// {
//     public class UserSession : BaseEntity
//     {
//         public Guid UserId { get; set; }
//         public DateTimeOffset SessionStart { get; set; }
//         public DateTimeOffset SessionEnd { get; set; }
//         public string RefreshTokenHash { get; set; } = string.Empty;
//         // public string UserAgent { get; set; } = string.Empty;
//         public ICollection<string> Devices { get; set; } = [];
//         public string IpAddress { get; set; }

//         // Navigation property to link to the user
//         public UserProfile? UserProfile { get; set; }
//     }
//     {
        
//     }
// }