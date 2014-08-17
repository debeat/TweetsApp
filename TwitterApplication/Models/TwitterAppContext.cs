using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TwitterApplication.Models
{
    public class TwitterAppContext : DbContext
    {
        public TwitterAppContext()
            : base("name=TwitterData")
        {
        }

        public DbSet<UsersInfo> UsersTable { get; set; }
        public DbSet<TweetsInfo> TweetsTable { get; set; }
    }
}