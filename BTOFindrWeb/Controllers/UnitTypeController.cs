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
    /// UnitTypeController web service class for retrieving and adding of unit types in database.
    /// Manage notification of newly added unit type to user that has subscription of the specific unit type.
    /// 
    /// Author: Calvin Che Zi Yi
    /// </summary>
    public class UnitTypeController : ApiController, UnitTypePublisher
    {
        /// <summary>
        /// SQL connection string from web.config
        /// </summary>
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        /// <summary>
        /// Method to get unit types.
        /// </summary>
        /// <returns>A list of strings that contain the unit types</returns>
        [HttpGet]
        public List<string> GetUnitTypes()
        {
            // Initialize a list of strings
            List<string> types = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for all unit types without duplicates in ascending order
                    String query = "SELECT DISTINCT UnitTypeName FROM UnitTypes ORDER BY UnitTypeName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                // If a row is returned, add the string value into the list of strings
                                types.Add(dr["UnitTypeName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return the list of strings that contain the unit types
            return types;
        }

        /// <summary>
        /// Method to get unit type information.
        /// </summary>
        /// <param name="unitTypeId">The id of a unit type</param>
        /// <returns>A UnitType object with relevant information</returns>
        [HttpGet]
        public UnitType GetUnitType(int unitTypeId)
        {
            // Initialize a UnitType object
            UnitType unitType = new UnitType();

            if (unitTypeId == 0)
            {
                // Return empty unit type if id is 0
                return unitType;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for unit type with the specific unit type id
                    String query = "SELECT * FROM UnitTypes WHERE UnitTypeId = @UnitTypeId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeId", unitTypeId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // If a row is returned, set all unit type information respectively
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
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return the UnitType object
            return unitType;
        }

        /// <summary>
        /// Method to get all unit types in a particular block.
        /// </summary>
        /// <param name="blockId">The id of a block</param>
        /// <returns>A list of UnitType object within the particular block</returns>
        [HttpGet]
        public List<UnitType> GetUnitTypesInBlock(int blockId)
        {
            // Initialize a list of UnitType objects
            List<UnitType> unitTypes = new List<UnitType>();

            if (blockId == 0)
            {
                // Return empty unit type if id is 0
                return unitTypes;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for all unit types with the specific block id
                    String query = "SELECT * FROM UnitTypes WHERE BlockId=@BlockId ORDER BY UnitTypeName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockId", blockId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                // If a row is returned, initialize a UnitType object
                                UnitType u = new UnitType();
                                u.unitTypeName = dr["UnitTypeName"].ToString();
                                u.unitTypeId = Convert.ToInt32(dr["UnitTypeId"]);
                                u.quotaChinese = Convert.ToInt32(dr["QuotaChinese"]);
                                u.quotaMalay = Convert.ToInt32(dr["QuotaMalay"]);
                                u.quotaOthers = Convert.ToInt32(dr["QuotaOthers"]);
                                // Add UnitType object into the list of unit types
                                unitTypes.Add(u);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return the list of UnitType objects
            return unitTypes;
        }

        /// <summary>
        /// Method to add unit type into database. If it exists in the database, return its unit type id.
        /// </summary>
        /// <param name="unitType">A UnitType object to be added into the database</param>
        /// <returns>The respective unit type id for the inserted unit type</returns>
        [HttpPost]
        public int AddUnitType(UnitType unitType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for any existing unit type with the same block id and unit type name
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
                                // If a row is returned, return the retrieved unit type id
                                return Convert.ToInt32(dr["UnitTypeId"]);
                            }
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query to insert unit type into database
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

                        // Return the unit type id
                        return (Int32)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurred, return -1
                return -1;
            }
        }

        /// <summary>
        /// Method to add subscription for users that indicated their interest in the particular unit type
        /// </summary>
        /// <param name="unitTypeName">A string that contains the unit type name</param>
        /// <param name="deviceId">A string that contains the device id</param>
        /// <returns>A bool true or false to indicate subscription</returns>
        [HttpGet]
        public bool Subscribe(string unitTypeName, string deviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for unit type subscriptions with specific unit type name and device id
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
                                // If a row is returned, return true if there is existing subscription
                                return true;
                            }
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query to insert unit type subscription into database
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
                // If an error occurred, return false
                return false;
            }
            // If unit type subscription is inserted successfully, return true
            return true;
        }

        /// <summary>
        /// Method to remove subscription for users that indicated their interest in the particular unit type 
        /// </summary>
        /// <param name="unitTypeName">A string that contains the unit type name</param>
        /// <param name="deviceId">A string that contains the device id</param>
        /// <returns>A bool true or false to indicate subscription</returns>
        [HttpGet]
        public bool Unsubscribe(string unitTypeName, string deviceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query to remove unit type subscription from database
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
                // If an error occurred, return false
                return false;
            }
            // If unit type subscription is removed successfully, return true
            return true;
        }

        /// <summary>
        /// Method to get subscriptions for user by their device id.
        /// </summary>
        /// <param name="deviceId">The id of a device</param>
        /// <returns>A list of string that contains unit types that user subscribed</returns>
        [HttpGet]
        public List<string> GetSubscriptions(string deviceId)
        {
            // Initialize a list of string
            List<string> unitTypeNames = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for all subscriptions for the particular device id
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
                                // If a row is returned, add unit type name into the list of unit type names
                                unitTypeNames.Add(dr["UnitTypeName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            // Return the list of unit type names
            return unitTypeNames;

        }

        /// <summary>
        /// Method to inform user of their subscriptions using Firebase Cloud Messaging (FCM)
        /// </summary>
        /// <param name="unitType">A UnitType object of a unit that was newly added</param>
        /// <returns>A bool true or false to indicate status of notification</returns>
        public bool Notify(UnitType unitType)
        {
            // Initialize a list of string
            List<string> deviceIds = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for subscriptions for the particular unit type
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
                                // If a row is returned, add the device id into the list of string
                                deviceIds.Add(dr["DeviceId"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurred, return false
                return false;
            }

            // Form the notification content
            string content = "New " + unitType.unitTypeName + " units available at " + unitType.block.blockNo + " " + unitType.block.project.projectName + "!";

            foreach (string deviceId in deviceIds)
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
                    // Send message to user's device using FCM
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Headers.Add(HttpRequestHeader.Authorization, "key=AIzaSyB2t8hQOj1o6zPK6-TBdk3XkpnKwMXnN8Y");
                    byte[] response = client.UploadData("https://fcm.googleapis.com/fcm/send", "POST", Encoding.UTF8.GetBytes(message));
                }
            }
            // If notification is successfully sent, return true
            return true;
        }
    }
}
