using BlogLife.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogLife.Helpers
{
    public class UserHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public string GetDisplayName(string userId)
        {
            var user = db.Users.Find(userId);
            return user.DisplayName;
        }
    }
}