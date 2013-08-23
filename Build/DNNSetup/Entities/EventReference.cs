using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    [DataContract]
    public class EventReference
    {

        public EventReference()
        {
            NewInviteeList = new int[0];
            InviteType = 1;//All Friends
        }

        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        [DataMember(Name = "topicId")]
        public int EventId { get; set; }

        [DataMember(Name = "groupId")]
        public int GroupId { get; set; }

        [DataMember(Name = "approved")]
        public bool Approved { get; set; }

        [DataMember(Name = "contentTitle")]
        public string ContentTitle { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "startTime")]
        public DateTime StartTime { get; set; }

        [DataMember(Name = "endTime")]
        public DateTime EndTime { get; set; }

        [DataMember(Name = "street")]
        public string Street { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "region")]
        public string Region { get; set; }

        [DataMember(Name = "postalCode")]
        public string PostalCode { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "maxAttendees")]
        public int MaxAttendees { get; set; }

        [DataMember(Name = "enableRsvp")]
        public bool EnableRsvp { get; set; }

        [DataMember(Name = "showGuests")]
        public bool ShowGuests { get; set; }

        [DataMember(Name = "inviteType")]
        public int InviteType { get; set; }

        [DataMember(Name = "newInviteeList")]
        public int[] NewInviteeList { get; set; }
    }
}
