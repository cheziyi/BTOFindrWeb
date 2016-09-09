using BTOFindrWeb.Models;
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
    public class ProjectController : ApiController
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;


        [HttpGet]
        public IEnumerable<string> GetTownNames()
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


        [HttpGet]
        public Project GetProject(string projectId)
        {
            Project project = new Project();

            if (projectId == null)
            {
                return project;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM Projects WHERE ProjectId = @ProjectId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectId", projectId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                project.projectId = dr["ProjectId"].ToString();
                                project.projectName = dr["ProjectName"].ToString();
                                project.townName = dr["TownName"].ToString();
                                project.ballotDate = dr["BallotDate"].ToString();
                                project.projectImage = dr["ProjectImage"].ToString();

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return project;
        }
    }
}
