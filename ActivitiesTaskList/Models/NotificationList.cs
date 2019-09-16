using System;
using System.Collections.Generic;

namespace ActivitiesTaskList.Models
{
    public partial class NotificationList
    {
        public int Id { get; set; }
        public int? ActivityId { get; set; }
        public string PhoneNumber { get; set; }

        public virtual Activities Activity { get; set; }
    }
}
