using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace TwitterApplication.Models
{
    public class TweetsInfo
    {
        public int TweetsInfoID { get; set; } 
        public long TweetID { get; set; }
        public string TweetText { get; set; }
        public DateTime CreationDate { get; set; }
        public string Location { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public virtual UsersInfo Useritem { get; set; }

    }
}