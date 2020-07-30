using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogLife.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //Nav
        public virtual ICollection<BlogPost> BlogPosts { get; set; }
        public virtual ICollection<Category> Categories { get; set; }

        public Category()
        {
            BlogPosts = new HashSet<BlogPost>();
        }
    }
}