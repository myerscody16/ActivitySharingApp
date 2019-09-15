using System;
using System.Collections.Generic;

namespace ActivitiesTaskList.Models
{
    public partial class UserToUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FriendId { get; set; }

        public virtual AspNetUsers Friend { get; set; }
        public virtual AspNetUsers User { get; set; }
    }
}
