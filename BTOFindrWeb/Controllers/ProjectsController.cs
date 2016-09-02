using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BTOFindrWeb.Controllers
{
    public class ProjectsController : ApiController
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;


        [HttpGet]
        public IEnumerable<string> Towns()
        {
            List<string> towns = new List<string>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                String query = "SELECT DISTINCT TownName FROM Projects ORDER BY TownName ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            towns.Add(dr["TownName"].ToString());
                        }
                    }
                }
            }
            return towns;
        }

        // GET: api/Projects/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Projects
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Projects/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Projects/5
        public void Delete(int id)
        {
        }
    }
}
