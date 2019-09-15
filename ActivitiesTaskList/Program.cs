using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ActivitiesTaskList
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string accountSid = "ACa789c5fec567f04e0ab72683617dd828";
            const string authToken = "4d148282e12613c36b8500584f592976";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: "It worked!!!",
                from: new Twilio.Types.PhoneNumber("+13134665096"),
                to: new Twilio.Types.PhoneNumber("+12485080655")
            );

            Console.WriteLine(message.Sid);
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        
    }
}
