using BTOFindr.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace BTOFindrWeb.Controllers
{
    /// <summary>
    /// UnitTypePublisher interface for Firebase Cloud Messaging (FCM)
    /// 
    /// Author: Calvin Che Zi Yi
    /// </summary>
    interface UnitTypePublisher
    {
        bool Subscribe(string unitTypeName, string deviceId);

        bool Unsubscribe(string unitTypeName, string deviceId);

        List<string> GetSubscriptions(string deviceId);

        bool Notify(UnitType unitType);
    }
}