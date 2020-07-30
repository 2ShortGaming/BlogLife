using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BlogLife.Helpers;
using BlogLife.Models;
using PagedList;
using PagedList.Mvc;

namespace BlogLife.Controllers
{
    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BlogPosts
        //[Authorize(Roles = "Admin")]
        public ActionResult Index(int? page, string searchStr)
        {
            ViewBag.Search = searchStr;
            var blogList = IndexSearch(searchStr);

            int pageSize = 5; //specifies the number of post per page
            int pageNumber = (page ?? 1);//?? null coalescing operator

            IPagedList<BlogPost> allBlogPosts = blogList.ToPagedList(pageNumber, pageSize);
            return View(allBlogPosts);
        }
        public IQueryable<BlogPost> IndexSearch(string searchStr)
        {
            IQueryable<BlogPost> result = null;
            if (searchStr != null)
            {
                result = db.BlogPosts.AsQueryable();
                result = result.Where(b => b.Title.Contains(searchStr) ||
                    b.Body.Contains(searchStr) ||
                    b.Comments.Any(c => c.Body.Contains(searchStr) ||
                        c.Author.FirstName.Contains(searchStr) ||
                        c.Author.LastName.Contains(searchStr) ||
                        c.Author.DisplayName.Contains(searchStr) ||
                        c.Author.Email.Contains(searchStr)));
            }
            else
            {
                result = db.BlogPosts.AsQueryable();
            }
            return result.OrderByDescending(b => b.Created);
        }

        // GET: BlogPosts/Details/5
        public ActionResult Details(string slug)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.FirstOrDefault(b => b.Slug == slug);
            BlogPost previousPost = db.BlogPosts.OrderByDescending(b => b.Created).ToList().SkipWhile(b => b.Created != blogPost.Created).Skip(1).FirstOrDefault();
            BlogPost nextPost = db.BlogPosts.OrderBy(b => b.Created).ToList().SkipWhile(b => b.Created != blogPost.Created).Skip(1).FirstOrDefault();

            if (previousPost == null)
            {
                ViewBag.PreviousPost = "No Earlier Posts";
            }
            else
            {
                ViewBag.PreviousPost = previousPost.Title;

            }
            if (nextPost == null)
            {
                ViewBag.NextPost = "No Newer Post";
            }
            else
            {
                ViewBag.NextPost = nextPost.Title ?? "No Newer Post";
            }

            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }
        public ActionResult PreviousPost (bool prev, int id)
        {
            if (prev)
            {
                BlogPost currentPost = db.BlogPosts.Find(id);
                BlogPost previousPost = db.BlogPosts.OrderByDescending(b => b.Created).ToList().SkipWhile(b => b.Created != currentPost.Created).Skip(1).FirstOrDefault();
                if (previousPost == null)
                {
                    return RedirectToAction(currentPost.Slug, "Blog/Details");
                }
                return RedirectToAction(previousPost.Slug, "Blog/Details");
            }
            else
            {
                BlogPost currentPost = db.BlogPosts.Find(id);
                BlogPost nextPost = db.BlogPosts.OrderBy(b => b.Created).ToList().SkipWhile(b => b.Created != currentPost.Created).Skip(1).FirstOrDefault();
                if (nextPost == null)
                {
                    return RedirectToAction(currentPost.Slug, "Blog/Details");
                }
                return RedirectToAction(nextPost.Slug, "Blog/Details");

            }
        }

       
        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Body,Abstract,MediaPath,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View(blogPost);
                }
                if (db.BlogPosts.Any(b => b.Slug == Slug))
                {
                    ModelState.AddModelError("Title", "The title must be unique");
                    return View(blogPost);
                }
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaPath = "/Uploads/" + fileName;
                }
                blogPost.Slug = Slug;
                blogPost.Created = DateTime.Now;
                db.BlogPosts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Title,Slug,Body,Abstract,MediaPath,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var slug = StringUtilities.URLFriendly(blogPost.Title);
                if (slug != blogPost.Slug)
                {
                    if (String.IsNullOrWhiteSpace(slug))
                {
                        ModelState.AddModelError("Title", "Invalid title");
                        return View(blogPost);
                }
                    if (db.BlogPosts.Any(b => b.Slug == slug))
                {
                         ModelState.AddModelError("Title", "The title must be unique");
                         return View(blogPost);
                }
                    if (ImageUploadValidator.IsWebFriendlyImage(image))
                    {
                        var fileName = Path.GetFileName(image.FileName);
                        var justFileName = Path.GetFileNameWithoutExtension(fileName);
                        justFileName = StringUtilities.URLFriendly(justFileName);
                        fileName = $"{justFileName}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
                        image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                        blogPost.MediaPath = "/Uploads/" + fileName;
                    }
                    blogPost.Slug = slug;
                }
                
                blogPost.Updated = DateTime.Now;
                db.Entry(blogPost).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.BlogPosts.Find(id);
            db.BlogPosts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
