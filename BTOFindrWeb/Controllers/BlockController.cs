using BTOFindrWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;

namespace BTOFindrWeb.Controllers
{
    public class BlockController : ApiController
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        [HttpPost]
        public IEnumerable<Block> SearchBlocks(SearchParameters paras)
        {
            List<Block> blocks = new List<Block>();


            String query = "SELECT Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, Blocks.SitePlan, Blocks.TownMap, ";
            query += "Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs, MIN(Units.Price) AS MinPrice, MAX(Units.Price)AS MaxPrice ";
            query += "FROM Blocks INNER JOIN ";
            query += "Projects ON Blocks.ProjectId = Projects.ProjectId INNER JOIN ";
            query += "UnitTypes ON Blocks.BlockId = UnitTypes.BlockId INNER JOIN ";
            query += "Units ON UnitTypes.UnitTypeId = Units.UnitTypeId WHERE ";

            if (paras.townNames.Length > 0)
            {
                query += "( ";
                for (int i = 0; i < paras.townNames.Length; i++)
                {
                    query += "(Projects.TownName = '" + paras.townNames[i] + "') ";
                    if (i != paras.townNames.Length - 1)
                        query += "OR ";
                    else
                        query += ") AND ";
                }
            }

            if (paras.ethnicGroup == 'C')
                query += "(UnitTypes.QuotaChinese > 0) AND ";
            else if (paras.ethnicGroup == 'M')
                query += "(UnitTypes.QuotaMalay > 0) AND ";
            else if (paras.ethnicGroup == 'O')
                query += "(UnitTypes.QuotaOthers > 0) AND ";


            if (paras.roomTypes.Length > 0)
            {
                query += "( ";
                for (int i = 0; i < paras.roomTypes.Length; i++)
                {
                    query += "(UnitTypes.UnitTypeName = '" + paras.roomTypes[i] + "') ";
                    if (i != paras.roomTypes.Length - 1)
                        query += "OR ";
                    else
                        query += ") AND ";
                }
            }

            query += "(Units.Price > " + paras.minPrice + ") AND (Units.Price < " + paras.maxPrice + ") ";

            query += "GROUP BY Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, ";
            query += "Blocks.SitePlan, Blocks.TownMap, Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs ";


            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Block b = new Block();
                            b.blockId = Convert.ToInt32(dr["BlockId"]);
                            b.blockNo = dr["BlockNo"].ToString();
                            b.street = dr["Street"].ToString();
                            b.deliveryDate = Convert.ToDateTime(dr["DeliveryDate"]);
                            b.locLat = Convert.ToDecimal(dr["LocLat"]);
                            b.locLong = Convert.ToDecimal(dr["LocLong"]);
                            b.sitePlan = dr["SitePlan"].ToString();
                            b.townMap = dr["TownMap"].ToString();
                            b.blockPlan = dr["BlockPlan"].ToString();
                            b.unitDist = dr["UnitDist"].ToString();
                            b.floorPlan = dr["FloorPlan"].ToString();
                            b.layoutIdeas = dr["LayoutIdeas"].ToString();
                            b.specs = dr["Specs"].ToString();
                            b.minPrice = Convert.ToDecimal(dr["MinPrice"]);
                            b.maxPrice = Convert.ToDecimal(dr["MaxPrice"]);
                            b.project = new Project();
                            b.project.projectId = dr["ProjectId"].ToString();

                            blocks.Add(b);
                        }
                    }
                }
            }

            ProjectController pc = new ProjectController();
            foreach (Block b in blocks)
            {
                b.project = pc.GetProject(b.project.projectId);
                b.travelTime = CalculateTravellingTime(b.locLat, b.locLong, paras.originPlaceId);
            }

            return blocks;
        }



        private int CalculateTravellingTime(decimal locLat, decimal locLong, string originPlaceId)
        {
            int time = 0;
            string uri = "https://maps.googleapis.com/maps/api/distancematrix/xml?units=metric";
            string key = "&key=AIzaSyAx2cHrI8CjdzkiByY_FS1nV93CFx9LD54";
            string origin = "&origins=" + locLat + "," + locLong;
            string destination = "&destinations=place_id:" + originPlaceId;

            XmlTextReader reader = new XmlTextReader(uri + key + origin + destination);
            reader.ReadToFollowing("duration");
            reader.ReadToFollowing("value");
            time = Convert.ToInt32(reader.ReadElementContentAsString());

            return time;
        }
    }
}
