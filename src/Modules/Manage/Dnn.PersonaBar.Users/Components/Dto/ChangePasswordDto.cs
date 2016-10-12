using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class ChangePasswordDto
    {
        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}