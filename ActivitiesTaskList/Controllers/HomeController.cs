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

namespace ActivitiesTaskList.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly string apikey;
        private readonly SharedActivityDbContext _context;
        private readonly IConfiguration _configuration;
        public HomeController(SharedActivityDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            apikey = _configuration.GetSection("Appconfiguration")["APIkeyvalue"];
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ActivityList()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<> databaseList = _context..ToList();

            List<> newTask = new List<>();
            AspNetUsers thisUser = _context.AspNetUsers.Where(u => u.UserName == User.Identity.Name).First();

            foreach (var item in databaseList)
            {
                if (id == item.UserId)
                {
                    newTask.Add(item);
                }
            }
            return View(newTask);
        }
        public IActionResult AddTask()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddTask()
        {
            string Id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            .UserId = Id;
            if (ModelState.IsValid)
            {
                _context..Add();
                _context.SaveChanges();
            }

            return RedirectToAction("");
        }
        public IActionResult Delete(int Id)
        {
            found = _context.TaskList.Find(Id);
            if (ModelState.IsValid)
            {
                _context..Remove(found);
                _context.SaveChanges();
            }

            return RedirectToAction("");
        }
        public IActionResult Update()
        {
            found = _context..Find(.UserId);
            if (ModelState.IsValid && found != null)
            {
                found.Complete = "yes";

                _context.Entry().State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.Update(found);
                _context.SaveChanges();
            }

            return RedirectToAction("");

        }
    }
}
