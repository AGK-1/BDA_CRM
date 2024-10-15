using Microsoft.AspNetCore.Identity;

namespace BDA.Models
{
    public class Userko: IdentityUser
    {  
          
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
