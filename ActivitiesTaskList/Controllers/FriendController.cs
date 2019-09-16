using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivitiesTaskList.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActivitiesTaskList.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly SharedActivityDbContext _context;
        public FriendController(SharedActivityDbContext context)
        {
            //Links Db
            //If no relations add default to prevent null references
            if (context.UserToUser == null)
            {
                context.UserToUser.Add(new UserToUser() { UserId = "0", FriendId = "0" });
            }
            context.SaveChanges();
            _context = context;
        }
        //public IActionResult Index()
        //{
        //    AspNetUsers thisUser = _context.AspNetUsers.Where(u => u.UserName == User.Identity.Name).First();
        //    var friendList = _context.UserToUser.Where(u => u.UserId == thisUser.Id).ToList();

        //    List<AspNetUsers> users = new List<AspNetUsers>();
        //    foreach (var friend in friendList)
        //    {
        //        var person = _context.AspNetUsers.Where(u => u.Id == friend.FriendId).First();
        //        users.Add(new AspNetUsers() { Id = person.Id, Email = person.Email, UserName = person.UserName });
        //    }
        //    return View("PLACEHOLDER", users);
        //}
        public IActionResult AddFriend(string friendId)
        {
            //Finds Both Users
            var currentUser = _context.AspNetUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var friend = _context.AspNetUsers.FirstOrDefault(u => u.Id == friendId);
            //friend = _context.AspNetUsers.First(u => u.Id == friend.ToString());

            //if (friend != null)
            //{
            //Create User Relation
            _context.UserToUser.Add(new UserToUser() { UserId = currentUser.Id, FriendId = friend.Id });
            _context.SaveChanges();
            //}            //Redirect to Index
            return RedirectToAction("FriendSuggestions");
        }
        public IActionResult RemoveFriend(string friendId)
        {
            var currentUser = _context.AspNetUsers.Where(u => u.UserName == User.Identity.Name).First();

            foreach (var relation in _context.UserToUser.ToList())
            {
                if (relation.FriendId == currentUser.Id && relation.UserId == friendId)
                {
                    _context.UserToUser.Remove(relation);
                }
                if (relation.FriendId == friendId && relation.UserId == currentUser.Id)
                {
                    _context.UserToUser.Remove(relation);
                }

            }

            _context.SaveChanges();
            return RedirectToAction("PLACEHOLDER");

        }
        internal List<AspNetUsers> SuggestFriends()
        {
            var currentUser = _context.AspNetUsers.First(u => u.UserName == User.Identity.Name);
            List<string> currentFriendIds = new List<string>();
            foreach (var relation in _context.UserToUser.ToList())
            {
                if (!currentFriendIds.Contains(relation.FriendId) && relation.UserId == currentUser.Id)
                {
                    currentFriendIds.Add(relation.FriendId);
                }
                if (!currentFriendIds.Contains(relation.UserId) && relation.FriendId == currentUser.Id)
                {
                    currentFriendIds.Add(relation.UserId);
                }
            }
            List<AspNetUsers> suggestions = new List<AspNetUsers>();
            List<Activities> userFavorites = new List<Activities>();
            foreach (var relation in _context.UserToActivity.ToList())
            {
                if (relation.UserId == currentUser.Id)
                {
                    var activity = _context.Activities.First(a => a.Id == relation.ActivityId);
                    if (!userFavorites.Contains(activity) && activity != null)
                    {
                        userFavorites.Add(activity);
                    }
                }
            }
            foreach (var activity in userFavorites)
            {
                var likeRelations = _context.UserToActivity.Where(r => r.ActivityId == activity.Id && r.UserId != currentUser.Id).ToList();
                if (likeRelations != null)
                {
                    foreach (var likeRelation in likeRelations)
                    {
                        var suggestion = _context.AspNetUsers.First(u => u.Id == likeRelation.UserId);
                        if (!currentFriendIds.Contains(likeRelation.UserId) && !suggestions.Contains(suggestion) && suggestion != null)
                        {
                            suggestions.Add(suggestion);
                        }
                    }
                }

            }
            return suggestions;
        }
        public IActionResult FriendSuggestions()
        {
            List<AspNetUsers> users = SuggestFriends();

            return View("FriendSuggestions", users);
        }
    }
}
