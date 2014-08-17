using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetSharp;
using WebGrease;
using System.Data.Entity;
using TwitterApplication.Models;
using System.Web.Script.Serialization;
using ExtensionMethods;


namespace TwitterApplication.Controllers
{
    public class HomeController : Controller
    {
        TwitterAppContext TwitterDB = new TwitterAppContext();
        private void GetOldTweets()
        {
            //TwitterService("consumer key", "consumer secret");
            //AuthenticateWith("Access Token", "AccessTokenSecret");
            var service = new TwitterService("UNrDr7NEn3yl9hro0xt0ocMxy", "1gDaYaKVJqPkPHXCkiRoLtp5COfeKIfD6vcswUz27vQUzQx3yC");
            service.AuthenticateWith("265655738-PWXyCPdrv0t1qFNHM1W3hRXdPU7H08oizz5kGdDG", "spauwDp6woHF1ClpGq8t6b3hzRTJsZTpDe6mtY0FEWpOq");

            bool keepLooping = true;
            long loopingID = 0;
            List<TwitterStatus> oldTweets = new List<TwitterStatus>();
            TwitterStatus lastTweet = new TwitterStatus();
            SearchOptions options = new SearchOptions { Q = "#acme", Count = 5 };
            TwitterSearchResult searchResults = service.Search(options);
            oldTweets = searchResults.Statuses.ToList();
            loopingID = oldTweets[0].Id;
            while (keepLooping)
            {
                try
                {
                    options = new SearchOptions { Q = "#acme", MaxId = loopingID, Count = 100 };
                    searchResults = service.Search(options);
                    oldTweets.AddRange(service.Search(options).Statuses);
                    lastTweet = oldTweets[oldTweets.Count - 1];
                    if (loopingID != lastTweet.Id)
                    {
                        loopingID = lastTweet.Id;
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    keepLooping = false;
                }
            }
            insertDB(oldTweets);
        }

        private void GetNewTweets()
        {
            //TwitterService("consumer key", "consumer secret");
            //AuthenticateWith("Access Token", "AccessTokenSecret");
            var service = new TwitterService("UNrDr7NEn3yl9hro0xt0ocMxy", "1gDaYaKVJqPkPHXCkiRoLtp5COfeKIfD6vcswUz27vQUzQx3yC");
            service.AuthenticateWith("265655738-PWXyCPdrv0t1qFNHM1W3hRXdPU7H08oizz5kGdDG", "spauwDp6woHF1ClpGq8t6b3hzRTJsZTpDe6mtY0FEWpOq");

            List<TwitterStatus> newTweets = new List<TwitterStatus>();
            IEnumerable<TweetsInfo> sortedList;
            SearchOptions options;
            TwitterSearchResult searchResults;
            bool keepLooping = true;
            long currentID = 0;
            while (keepLooping)
            {
                try
                {
                    sortedList = TwitterDB.TweetsTable.ToList().OrderBy(x => x.CreationDate);
                    var newestItem = sortedList.First();
                    if (currentID != newestItem.TweetID)
                    {
                        currentID = newestItem.TweetID;
                    }
                    else
                    {
                        break;
                    }
                    options = new SearchOptions { Q = "#acme", SinceId = currentID, Count = 100 };
                    searchResults = service.Search(options);
                    newTweets.AddRange(searchResults.Statuses);
                }
                catch
                {
                    keepLooping = false;
                }
            }
            if (newTweets != null)
            {
                insertDB(newTweets);
            }
        }

        private void insertDB(List<TwitterStatus> Tweets){
            string lat;
            string lon;
            string loc;
            foreach (var tweet in Tweets)
            {
                try
                {
                    lat = tweet.Location.Coordinates.Latitude.ToString();
                    lon = tweet.Location.Coordinates.Longitude.ToString();
                }
                catch
                {
                    lat = "";
                    lon = "";
                }
                try
                {
                    loc = tweet.Location.ToString();
                }
                catch
                {
                    loc = "";
                }
                var user = TwitterDB.UsersTable.SingleOrDefault(u => u.ScreenName == tweet.User.ScreenName);
                var twt = TwitterDB.TweetsTable.SingleOrDefault(t => t.TweetID == tweet.Id);
                if (user == null)
                {
                    var newUser = new UsersInfo
                    {
                        UsersInfoID = tweet.User.Id,
                        ScreenName = tweet.User.ScreenName,
                        ProfileImageUrl = tweet.User.ProfileImageUrl,
                        Name = tweet.User.Name,
                        TweetsCounter = 1,
                        Description = tweet.User.Description,
                        Location = tweet.User.Location,
                    };
                    var newTweet = new TweetsInfo
                    {
                        TweetID = tweet.Id,
                        TweetText = tweet.Text,
                        CreationDate = tweet.CreatedDate,
                        Longitude = lon,
                        Latitude = lat,
                        Location = loc,
                        Useritem = newUser
                    };
                    TwitterDB.UsersTable.Add(newUser);
                    TwitterDB.TweetsTable.Add(newTweet);
                    TwitterDB.SaveChanges();
                }
                else
                {
                    if (twt == null)
                    {
                        var newTweet = new TweetsInfo
                        {
                            TweetID = tweet.Id,
                            TweetText = tweet.Text,
                            CreationDate = tweet.CreatedDate,
                            Longitude = lon,
                            Latitude = lat,
                            Location = loc,
                            Useritem = user
                        };
                        TwitterDB.TweetsTable.Add(newTweet);
                        user.TweetsCounter++;
                        TwitterDB.Entry(user).State = EntityState.Modified;
                        TwitterDB.SaveChanges();
                    }
                }
            }
        }

        private void createMap()
        {
            IEnumerable<TweetsInfo> sortTweets = TwitterDB.TweetsTable.ToList().OrderBy(x => x.CreationDate);
            List<MapItems> map = new List<MapItems>();
            foreach (var i in sortTweets)
            {
                if (!String.IsNullOrEmpty(i.Latitude))
                {
                    var item = new MapItems
                            {
                                latitude = i.Latitude,
                                longtude = i.Longitude,
                                UserName = i.Useritem.Name,
                            };
                    map.Add(item);
                }
                else
                {
                    continue;
                }
            }
            string mapItemsString = map.ToJSON();
            ViewBag.MapData = mapItemsString;
        }

        private void createPieChart()
        {
            IEnumerable<UsersInfo> sortTweets = TwitterDB.UsersTable.ToList().OrderByDescending(x => x.TweetsCounter);
            List<PieChartItems> chart = new List<PieChartItems>();
            int top10Counter = 1;
            foreach (var i in sortTweets)
            {
                if (top10Counter >= 10)
                {
                    break;
                }
                var item = new PieChartItems
                {
                    UserScreenName = i.ScreenName,
                    UserTweetsAmount = i.TweetsCounter
                };
                chart.Add(item);
                top10Counter++;
            }
            string chartItemsString = chart.ToJSON();
            ViewBag.ChartData = chartItemsString;
        }

        [HttpGet]
        public ActionResult Index()
        {
            GetNewTweets();
            createMap();
            createPieChart();
            IEnumerable<TweetsInfo> sortTweets = TwitterDB.TweetsTable.ToList().OrderByDescending(x => x.CreationDate);
            return View(sortTweets);
        }
    }
}

namespace ExtensionMethods
    {
        public static class JSONHelper
        {
            public static string ToJSON(this object obj)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Serialize(obj);
            }
            public static string ToJSON(this object obj, int recursionDepth)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.RecursionLimit = recursionDepth;
                return serializer.Serialize(obj);
            }
        }
    }

