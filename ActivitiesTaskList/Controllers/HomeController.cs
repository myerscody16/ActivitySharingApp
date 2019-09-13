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

            return View();
        }

        public IActionResult AddTask(Activities newActivity)
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
        //public IActionResult Delete(int Id)
        //{
        //    found = _context.TaskList.Find(Id);
        //    if (ModelState.IsValid)
        //    {
        //        _context..Remove(found);
        //        _context.SaveChanges();
        //    }

        //    return RedirectToAction("");
        //}
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
