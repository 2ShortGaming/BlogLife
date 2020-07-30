using BlogLife.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogLife.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(int? page)
        {
            int pageSize = 5; //specifies the number of post per page
            int pageNumber = (page ?? 1);//?? null coalescing operator
            var allBlogPosts = db.BlogPosts.OrderByDescending(b => b.Created).Where(b => b.Published).ToPagedList(pageNumber, pageSize);
            return View(allBlogPosts);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            EmailModel model = new EmailModel();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
                    var from = "MyPortfolio<example@email.com>";
                    model.Body = "this message is from your blog site. The name and the email of  the contacting person is above.";

                    var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = "Blog Contact Message",
                        Body = string.Format(body, model.FromName, model.FromEmail, model.Body),
                        IsBodyHtml = true
                    };
                    var svc = new EmailService();
                    await svc.SendAsync(email);
                    return View(new EmailModel());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }

        public PartialViewResult _LoginPartial()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            return PartialView(user);
        }
    }
}