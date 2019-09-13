using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ActivitiesTaskList.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace ActivitiesTaskList.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        List<Activities> listActivities;
        private readonly string apikey;
        private readonly SharedActivityDbContext _context;
        private readonly IConfiguration _configuration;
        public HomeController(SharedActivityDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            apikey = _configuration.GetSection("Appconfiguration")["APIkeyvalue"];
            listActivities = _context.Activities.ToList();
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Results(string query)

        {
           
            var result = new List<Activities>();
            foreach (var activity in listActivities)
            {
                if (activity.Title.ToLower().Contains(query.ToLower()) && !result.Contains(activity))
                {
                    result.Add(activity);
                }
            }
            return View(result);
        }
        public IActionResult AddActivity()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddActivity(Activities newActivity)
        {
            string Id = _context.AspNetUsers.Where(u => u.UserName == User.Identity.Name).First().Id;
            newActivity.CreatedBy = Id;
            if (ModelState.IsValid)
            {
                _context.Activities.Add(newActivity);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult DeleteActivity(int Id)
        {
            var found = _context.Activities.Find(Id);
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
           
            if (found.CreatedBy == currentUser.Id)
            {
                _context.Activities.Remove(found);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        //public IActionResult Update()
        //{
        //    found = _context..Find(.UserId);
        //    if (ModelState.IsValid && found != null)
        //    {
        //        found.Complete = "yes";

        //        _context.Entry().State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //        _context.Update(found);
        //        _context.SaveChanges();
        //    }

        //    return RedirectToAction("");

        //}
    }
}
