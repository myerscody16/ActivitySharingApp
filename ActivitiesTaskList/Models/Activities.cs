using System;
using System.Collections.Generic;

namespace ActivitiesTaskList.Models
{
    public partial class Activities
    {
        public Activities()
        {
            UserToActivity = new HashSet<UserToActivity>();
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Cost { get; set; }
        public string CreatedBy { get; set; }

        public virtual AspNetUsers CreatedByNavigation { get; set; }
        public virtual ICollection<UserToActivity> UserToActivity { get; set; }
    }
}
