using BTOFindr.Models;
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
    /// <summary>
    /// ProjectController web service class for retrieving and adding of projects in database.
    /// </summary>
    public class ProjectController : ApiController
    {
        /// <summary>
        /// SQL connection string from web.config
        /// </summary>
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        /// <summary>
        /// Method to get town names.
        /// </summary>
        /// <returns>A list of strings that contain the town names</returns>
        [HttpGet]
        public List<string> GetTownNames()
        {
            // Initialize a list of strings
            List<string> towns = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for all town names without duplicates in ascending order
                    String query = "SELECT DISTINCT TownName FROM Projects ORDER BY TownName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                // If a row is returned, add the string value into the list of strings
                                towns.Add(dr["TownName"].ToString());
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
            // Return the list of strings that contain the town names
            return towns;
        }

        /// <summary>
        /// Method to get project information.
        /// </summary>
        /// <param name="projectId">The id of a project</param>
        /// <returns>A Project object with relevant information</returns>
        [HttpGet]
        public Project GetProject(string projectId)
        {
            // Initialize a Project object
            Project project = new Project();

            if (projectId == null)
            {
                // Return empty project if id is null
                return project;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for the particular project using the provided project id
                    String query = "SELECT * FROM Projects WHERE ProjectId = @ProjectId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectId", projectId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // If a row is found, set the relevant information into Project object
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
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return Project object
            return project;
        }

        /// <summary>
        /// Method to add project into database. If it exists in the database, return its project id.
        /// </summary>
        /// <param name="project">A Project object to be added into database</param>
        /// <returns>The respective project id for the inserted project</returns>
        [HttpPost]
        public string AddProject(Project project)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for any existing project with the same project name
                    String query = "SELECT * FROM Projects WHERE ProjectName=@ProjectName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectName", project.projectName);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                               // If a row is returned, return the retrieved project id
                               return dr["ProjectId"].ToString();
                            }
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query to insert project into the database
                    String query = "INSERT INTO Projects(ProjectId,ProjectName,TownName,BallotDate,ProjectImage) VALUES(@ProjectId,@ProjectName,@TownName,@BallotDate,@ProjectImage)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProjectId", project.projectId);
                        cmd.Parameters.AddWithValue("@ProjectName", project.projectName);
                        cmd.Parameters.AddWithValue("@TownName", project.townName);
                        cmd.Parameters.AddWithValue("@BallotDate", project.ballotDate);
                        cmd.Parameters.AddWithValue("@ProjectImage", project.projectImage);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    // Return the project id
                    return project.projectId;
                }
            }
            catch (Exception ex)
            {
                // If an error occurred, return null
                return null;
            }
        }
    }
}
