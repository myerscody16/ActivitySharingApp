using System;
using System.Collections.Generic;

namespace ActivitiesTaskList.Models
{
    public partial class Messages
    {
        public int Id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
    }
}
