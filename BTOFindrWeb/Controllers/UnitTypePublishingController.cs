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
    public class UnitTypePublishingController : ApiController
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        static public HttpClient client;

        [HttpGet]
        public bool Subscribe(string unitTypeName, string deviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM UnitTypeSubscriptions WHERE UnitTypeName=@UnitTypeName AND DeviceId=@DeviceId";

                    using (BlockController bc = new BlockController())
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeName", unitTypeName);
                        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                return true;
                            }
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "INSERT INTO UnitTypeSubscriptions(UnitTypeName,DeviceId) VALUES(@UnitTypeName,@DeviceId)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeName", unitTypeName);
                        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        [HttpGet]
        public bool Unsubscribe(string unitTypeName, string deviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "DELETE FROM UnitTypeSubscriptions WHERE UnitTypeName=@UnitTypeName AND DeviceId=@DeviceId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeName", unitTypeName);
                        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }


        [HttpGet]
        public List<string> GetSubscriptions(string deviceId)
        {
            List<string> unitTypeNames = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM UnitTypeSubscriptions WHERE DeviceId=@DeviceId";

                    using (BlockController bc = new BlockController())
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                unitTypeNames.Add(dr["UnitTypeName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return unitTypeNames;

        }

        public bool Notify(UnitType unitType)
        {
            List<string> deviceIds = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM UnitTypeSubscriptions WHERE UnitTypeName=@UnitTypeName";

                    using (BlockController bc = new BlockController())
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeName", unitType.unitTypeName);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                deviceIds.Add(dr["DeviceId"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            foreach (string deviceId in deviceIds)
                SendFCMMessage(deviceId, "New " + unitType.unitTypeName + " units available at " + unitType.block.blockNo + " " + unitType.block.project.projectName + "!");

            return true;

        }


        private bool SendFCMMessage(string deviceId, string content)
        {

            var message = JsonConvert.SerializeObject(new
            {
                to = deviceId,
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