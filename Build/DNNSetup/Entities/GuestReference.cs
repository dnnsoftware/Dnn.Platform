using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    public class GuestReference
    {
        #region Public Properties

        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        [DataMember(Name = "eventId")]
        public int EventId { get; set; }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "rsvpStatus")]
        public int RsvpStatus { get; set; }

        #endregion
    }
}
