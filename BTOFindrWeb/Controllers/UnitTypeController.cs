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
    public class UnitTypeController : ApiController, UnitTypePublisher
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        [HttpGet]
        public List<string> GetUnitTypes()
        {
            List<string> types = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT DISTINCT UnitTypeName FROM UnitTypes ORDER BY UnitTypeName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                types.Add(dr["UnitTypeName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return types;
        }

        [HttpGet]
        public UnitType GetUnitType(int unitTypeId)
        {
            UnitType unitType = new UnitType();

            if (unitTypeId == 0)
            {
                return unitType;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM UnitTypes WHERE UnitTypeId = @UnitTypeId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeId", unitTypeId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                unitType.unitTypeName = dr["UnitTypeName"].ToString();
                                unitType.unitTypeId = Convert.ToInt32(dr["UnitTypeId"]);
                                unitType.quotaChinese = Convert.ToInt32(dr["QuotaChinese"]);
                                unitType.quotaMalay = Convert.ToInt32(dr["QuotaMalay"]);
                                unitType.quotaOthers = Convert.ToInt32(dr["QuotaOthers"]);
                                unitType.block = new Block();
                                unitType.block.blockId = Convert.ToInt32(dr["BlockId"]);
                            }
                        }
                    }
                }

                using (BlockController bc = new BlockController())
                {
                    unitType.block = bc.GetBlock(unitType.block.blockId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return unitType;
        }


        [HttpGet]
        public List<UnitType> GetUnitTypesInBlock(int blockId)
        {
            List<UnitType> unitTypes = new List<UnitType>();

            if (blockId == 0)
            {
                return unitTypes;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM UnitTypes WHERE BlockId=@BlockId ORDER BY UnitTypeName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockId", blockId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                UnitType u = new UnitType();
                                u.unitTypeName = dr["UnitTypeName"].ToString();
                                u.unitTypeId = Convert.ToInt32(dr["UnitTypeId"]);
                                u.quotaChinese = Convert.ToInt32(dr["QuotaChinese"]);
                                u.quotaMalay = Convert.ToInt32(dr["QuotaMalay"]);
                                u.quotaOthers = Convert.ToInt32(dr["QuotaOthers"]);
                                unitTypes.Add(u);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return unitTypes;
        }

        [HttpPost]
        public int AddUnitType(UnitType unitType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM UnitTypes WHERE BlockId=@BlockId AND UnitTypeName=@UnitTypeName";

                    using (BlockController bc = new BlockController())
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockId", bc.AddBlock(unitType.block));
                        cmd.Parameters.AddWithValue("@UnitTypeName", unitType.unitTypeName);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                return Convert.ToInt32(dr["UnitTypeId"]);
                            }
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "INSERT INTO UnitTypes(BlockId,UnitTypeName,QuotaMalay,QuotaChinese,QuotaOthers) VALUES(@BlockId,@UnitTypeName,@QuotaMalay,@QuotaChinese,@QuotaOthers);";
                    query += "SELECT CAST(scope_identity() AS int)";
                    using (BlockController bc = new BlockController())
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockId", bc.AddBlock(unitType.block));
                        cmd.Parameters.AddWithValue("@UnitTypeName", unitType.unitTypeName);
                        cmd.Parameters.AddWithValue("@QuotaMalay", unitType.quotaMalay);
                        cmd.Parameters.AddWithValue("@QuotaChinese", unitType.quotaChinese);
                        cmd.Parameters.AddWithValue("@QuotaOthers", unitType.quotaOthers);
                        conn.Open();

                        Notify(unitType);

                        return (Int32)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

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
            {
                string content = "New " + unitType.unitTypeName + " units available at " + unitType.block.blockNo + " " + unitType.block.project.projectName + "!";
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
            return true;
        }
    }
}
