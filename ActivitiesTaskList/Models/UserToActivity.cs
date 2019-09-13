using System;
using System.Collections.Generic;

namespace ActivitiesTaskList.Models
{
    public partial class UserToActivity
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string UserId { get; set; }

        public virtual Activities Activity { get; set; }
        public virtual AspNetUsers User { get; set; }
    }
}
