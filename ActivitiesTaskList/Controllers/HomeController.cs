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
using Microsoft.EntityFrameworkCore;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Globalization;
using System.Threading;

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
        }
        #region Crud
        [AllowAnonymous]
        public IActionResult Index()
        {
            var listActivities = _context.Activities.ToList();
            return View(listActivities);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Results(string query)

        {
          if(query == ""|| query == " "|| query == null)
            {
                return RedirectToAction("Index");
            }
            var result = new List<Activities>();
            foreach (var activity in listActivities)
            {
                var val = CompareStrings(query, activity.Title);
                if (val && !result.Contains(activity))
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
            var found = _context.Activities.First(a=>a.Id == Id);
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
            
            if (found.CreatedBy == currentUser.Id)
            {
                foreach (var relation in _context.UserToActivity.Where(r => r.ActivityId == found.Id))
                {
                    _context.UserToActivity.Remove(relation);
                }
                _context.Activities.Remove(found);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        #endregion
        [HttpGet]
        public IActionResult AddPhoneToNotifyList(int activityId)
        {
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
            var activity = _context.Activities.First(a=>a.Id == activityId);
            if(currentUser != null && activity != null && activity.CreatedBy == currentUser.Id)
            {
                return View(new NotificationList() {ActivityId = activityId });
            }
            else
            {
                return Redirect("/Home/Index");
            }
        }
        [HttpPost]
        public IActionResult AddPhoneToNotifyList(NotificationList notification)
        {

            bool belongsToUser =(_context.AspNetUsers.Where(u => u.PhoneNumber == notification.PhoneNumber) != null);
             bool alreadyAdded =    (_context.NotificationList.Where(n => n.PhoneNumber == notification.PhoneNumber &&
                 n.ActivityId == notification.ActivityId) != null);
            bool isNotUserOrPreAdded = belongsToUser && alreadyAdded;
            if (isNotUserOrPreAdded)
            {
                _context.NotificationList.Add(notification);
                _context.SaveChanges();
               return RedirectToAction("Index");
            }
            ViewBag.Error = "Phone Number is already added or belongs to a user.";
            return View(notification);
        }
        public IActionResult SavedToList(int Id)
        {
            if (Id != 0)
            {
                var currentUser = _context.AspNetUsers.First(c => c.UserName == User.Identity.Name);
                _context.UserToActivity.Add(new UserToActivity() { ActivityId = Id, UserId = currentUser.Id });
                _context.SaveChanges();
            }

            return RedirectToAction("SavedActivities");
        }
        public IActionResult SavedActivities()
        {
            var currentUser = _context.AspNetUsers.First(c => c.UserName == User.Identity.Name);
            var savedActivities = _context.UserToActivity.Where(u => u.UserId == currentUser.Id).ToList();
            if (currentUser != null && savedActivities != null)
            {
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
            else
            {
                return View("Index");
            }
        }
        public IActionResult DeleteActivityFromUser(int id)
        {
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
            var found = _context.UserToActivity.First(u => u.ActivityId == id&&u.UserId==currentUser.Id);
            if (found.ActivityId == id && found.UserId == currentUser.Id)
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
            if (updatedActivity.Id != null)
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
            }
            return RedirectToAction("SavedActivities");
        }
        [HttpGet]
        public IActionResult DirectMessage(string friendId)
        {
            var friend = _context.AspNetUsers.First(u => u.Id == friendId);
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
            if(currentUser != null && friend != null)
            {
                return View(new Messages() { To = friend.PhoneNumber });
            }
            return Redirect("/Home/ListOfFriends");
            
        }
        [HttpPost]
        public IActionResult DirectMessage(Messages messages)
        {
            try
            {
                string accountSid = _configuration.GetSection("TwilioAccountDetails")["AccountSid"];
                string authToken = _configuration.GetSection("TwilioAccountDetails")["AuthToken"];
                if (accountSid != null && authToken != null)
                {
                        TwilioClient.Init(accountSid, authToken);
                        var message = MessageResource.Create(
                        body: $"{messages.Body} (Do not reply)",
                        from: new Twilio.Types.PhoneNumber("+13134665096"),
                        to: new Twilio.Types.PhoneNumber($"+1{messages.To}")
                    );
                    Console.WriteLine(message.Sid);
                }
                return Redirect("/Friend/ListOfFriends");
            }
            catch
            {
                ViewBag.Error = "Message could not be sent.";
                return View(messages);
            }
        }
        public IActionResult GetListOfFavoritedUsers(Activities favoriteActivity)
        {
            var UsersThatLikeActivity = _context.UserToActivity.Where(x => x.ActivityId == favoriteActivity.Id);

            return View(UsersThatLikeActivity);
        }
        public IActionResult SendReminder(int id)//should also bring in a list of all the users that have favorited this event
        {
            var UsersActivity = _context.Activities.First(a => a.Id == id);
            string date = UsersActivity.Date.ToString();

            var UsersThatLikeActivity = _context.UserToActivity.Where(x => x.ActivityId == UsersActivity.Id);
            List<AspNetUsers> userList = _context.AspNetUsers.ToList();
            List<AspNetUsers> favList = new List<AspNetUsers> { };
            foreach(var favUser in UsersThatLikeActivity)
            {
                var userId = favUser.UserId.ToString();
                foreach(var user in userList)
                {
                    if(user.Id == userId)
                    {
                        favList.Add(user);
                    }
                }
            }
            
             string accountSid = _configuration.GetSection("TwilioAccountDetails")["AccountSid"];
             string authToken = _configuration.GetSection("TwilioAccountDetails")["AuthToken"];
            if (accountSid != null && authToken != null)
            {
                TwilioClient.Init(accountSid, authToken);
                foreach (var user in favList)
                {
                    try
                    {
                        var message = MessageResource.Create(
                            body: $"Hello friend! This is a remind that the event: {UsersActivity.Title}, will be taking place on {date}. I hope to see you there! (Please do not repond to this message)",
                            from: new Twilio.Types.PhoneNumber("+13134665096"),
                            to: new Twilio.Types.PhoneNumber($"+1{user.PhoneNumber}")
                        );
                        Console.WriteLine(message.Sid);
                    }
                    catch { }
                }
            }
            var otherNotifyees = _context.NotificationList.Where(n => n.ActivityId == UsersActivity.Id).ToList();
            foreach (var number in otherNotifyees)
            {
                try
                {
                    var message = MessageResource.Create(
                        body: $"Hello friend! This is a remind that the event: {UsersActivity.Title}, will be taking place on {date}. I hope to see you there! (Please do not repond to this message)",
                        from: new Twilio.Types.PhoneNumber("+13134665096"),
                        to: new Twilio.Types.PhoneNumber($"+1{number.PhoneNumber}")
                    );
                    Console.WriteLine(message.Sid);
                }
                catch { }
            }
        
            return RedirectToAction("SavedActivities");
        }
        public static bool CompareStrings(string query, string compared)
        {
            var arrayQ = query.ToLower().ToCharArray();
            var arrayC = compared.ToLower().ToCharArray();
            double matches = 0;
            foreach(var letterQ in arrayQ)
            {
                foreach(var letterC in arrayC)
                {
                    if (letterC == letterQ)
                    {
                        matches++;
                    }
                }
            }
            double minMatch = ((double)compared.Length/(double) query.Length);
            var isPass = (matches / ((double)query.Length/(double)compared.Length) >= minMatch);
            if (isPass && matches !=0||compared.Contains(query))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
