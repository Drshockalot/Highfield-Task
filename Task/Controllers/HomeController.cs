using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Task.Models;
using RestSharp;
using Newtonsoft.Json;

// Renamed namespace due to poor choice of project name....and to avoid using global namespace
namespace Tasks.Controllers
{
    public class User {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public DateTime dob { get; set; }
        public int agePlusTwenty { get; set; }
        public string favouriteColour { get; set; }
    }

    public class Data
    {
        public string title { get; set; }
        public string details { get; set; }
        public string requestType { get; set; }
        public string uriToSubmit { get; set; }
        public string objectLayout { get; set; }
        public IEnumerable<User> data { get; set; }
    }

    public class PostData {
        public IEnumerable<int> AgePlusTwenty { get; set; }
        public IEnumerable<TopColour> TopColours { get; set; }
    }

    public class TopColour {
        public string Color { get; set; }
        public int Amount { get; set; }
    }

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        /// TASK CODE
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////

        public IActionResult Data()
        {
            // Get API data
            var restClient = new RestClient("http://recruitment.highfieldqualifications.com/");
            var request = new RestRequest("api/gettest");
            var result = restClient.Execute(request);
            var json = result.Content;

            // Deserialize and calculate new/top values values
            var dataObj = JsonConvert.DeserializeObject<Data>(json);
            var orderedColours = dataObj.data.GroupBy(x => x.favouriteColour).OrderByDescending(x => x.Count());
            var topColours = orderedColours.Select(x => { return new TopColour { Color = x.Key, Amount = x.Count() }; });

            // Assign to ViewData for use in view
            ViewBag.TopColours = topColours;
            ViewBag.Users = dataObj.data.Select(GetUserWithAgePlusTwenty);

            // Post requested data back to server
            var postData = new PostData
            {
                AgePlusTwenty = dataObj.data.Select(GetAgePlusTwenty),
                TopColours = topColours
            };

            request = new RestRequest("api/submitTest", Method.POST);
            request.AddBody(postData);
            result = restClient.Execute(request);

            return View();
        }

        // Calclation for view data
        public User GetUserWithAgePlusTwenty(User user) {
            var today = DateTime.Today;
            var baseAge = (today.Year - user.dob.Year) + 20;

            // If their birthday has not occurred yet this year, subtract one
            if (today.Month < user.dob.Month)
                --baseAge;
            else if (today.Month == user.dob.Month)
                if (today.Day < user.dob.Day)
                    --baseAge;

            user.agePlusTwenty = baseAge;

            return user;
        }

        // Calculation for post data
        public int GetAgePlusTwenty(User user)
        {
            var today = DateTime.Today;
            var baseAge = (today.Year - user.dob.Year) + 20;

            // If their birthday has not occurred yet this year, subtract one
            if (today.Month < user.dob.Month)
                --baseAge;
            else if (today.Month == user.dob.Month)
                if (today.Day < user.dob.Day)
                    --baseAge;

            return baseAge;
        }

        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
        //////////////////////////////////////////////////////
    }
}
