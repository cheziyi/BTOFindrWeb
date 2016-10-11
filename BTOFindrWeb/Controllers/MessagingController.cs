using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace BTOFindrWeb.Controllers
{


    public class MessagingController : ApiController
    {
        static public HttpClient client;

        [HttpGet]
        public bool SendFCMMessage(string topic, string content)
        {

            var message = JsonConvert.SerializeObject(new
            {
                to = "/topics/" + topic,
                notification = new
                {
                    body = content,
                    title = ""
                }
            });

            using (var client = new WebClient { UseDefaultCredentials = true })
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "key=AIzaSyB2t8hQOj1o6zPK6-TBdk3XkpnKwMXnN8Y");
                byte[] response = client.UploadData("https://fcm.googleapis.com/fcm/send", "POST", Encoding.UTF8.GetBytes(message));
                string result = client.Encoding.GetString(response);
                if (result.Contains("message_id"))
                    return true;
            }
            return false;
        }
    }
}
