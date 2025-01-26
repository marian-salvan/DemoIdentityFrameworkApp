using Microsoft.AspNetCore.Identity;

namespace DemoBackend.Entities
{
    public class UserEntity : IdentityUser
    {
        //extend the IdentityUser with your custom additional properties
        public int Age { get; set; }
    }
}
