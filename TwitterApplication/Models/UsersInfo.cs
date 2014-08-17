using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace TwitterApplication.Models
{
    public class UsersInfo
    {
        public long UsersInfoID { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int TweetsCounter { get; set; }
        public virtual List<TweetsInfo> UserTweets { get; set; }

    }
}