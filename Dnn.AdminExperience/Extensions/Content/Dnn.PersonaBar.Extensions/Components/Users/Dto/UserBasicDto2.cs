using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    public class UserBasicDto2 : UserBasicDto
    {
        public int TotalCount { get; set; }

        public UserBasicDto2()
        {
        }

        public UserBasicDto2(UserInfo user) : base(user)
        {
        }
    }
}
