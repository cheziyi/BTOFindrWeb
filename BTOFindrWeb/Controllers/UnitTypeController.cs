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
    public class UnitTypeController : ApiController
    {

        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;


        [HttpGet]
        public IEnumerable<string> GetUnitTypes()
        {
            List<string> types = new List<string>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                String query = "SELECT DISTINCT FlatTypeName FROM UnitTypes ORDER BY UnitTypeName ASC";
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
            return types;
        }
    }
}
