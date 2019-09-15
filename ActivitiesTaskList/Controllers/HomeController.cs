﻿using System;
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
using Microsoft.EntityFrameworkCore;

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
        #region Crud
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
                _context.UserToActivity.Add(new UserToActivity() { ActivityId = newActivity.Id, UserId = Id });
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
        #endregion

        public IActionResult SavedToList(int Id)
        {
            var currentUser = _context.AspNetUsers.First(c => c.UserName == User.Identity.Name);
            _context.UserToActivity.Add(new UserToActivity() { ActivityId = Id, UserId = currentUser.Id });
            _context.SaveChanges();


            return RedirectToAction("SavedActivities");
        }
        public IActionResult SavedActivities()
        {
            var currentUser = _context.AspNetUsers.First(c => c.UserName == User.Identity.Name);
            var savedActivities = _context.UserToActivity.Where(u => u.UserId == currentUser.Id).ToList();
            List<Activities> acts = new List<Activities>();

            foreach (var item in savedActivities)
            {
                var act = _context.Activities.First(a => a.Id == item.ActivityId);
                if (act != null)
                {
                    acts.Add(act);
                }
            }
            return View(acts);
        }
        public IActionResult DeleteActivityFromUser(int Id)
        {
            var found = _context.UserToActivity.First(u => u.ActivityId == Id);
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
            if (found.ActivityId == Id && found.UserId == currentUser.Id)
            {
                _context.UserToActivity.Remove(found);
                _context.SaveChanges();
            }
            return RedirectToAction("SavedActivities");
        }
        public IActionResult Update(int Id)
        {
            var found = _context.Activities.Find(Id);
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);

            if (found.Id == Id && found.CreatedBy == currentUser.Id)
            {
                return View(found);
            }
            else
            {
                return View("InvalidCredentials");
            }
        }
        [HttpPost]
        public IActionResult Update(Activities updatedActivity)
        {
            var found = _context.Activities.Find(updatedActivity.Id);
            if (ModelState.IsValid)
            {
                found.Title = updatedActivity.Title;
                found.Location = updatedActivity.Location;
                found.Date = updatedActivity.Date;
                found.Cost = updatedActivity.Cost;
                found.Description = updatedActivity.Description;
                _context.Entry(found).State = EntityState.Modified;
                _context.Update(found);
                _context.SaveChanges();
            }

            return RedirectToAction("SavedActivities");
        }
    }
}
