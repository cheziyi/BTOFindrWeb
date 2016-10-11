using BTOFindr.Models;
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
        public List<Block> SearchBlocks(SearchParameters searchParams)
        {
            List<Block> blocks = new List<Block>();


            String query = "SELECT Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, Blocks.SitePlan, Blocks.TownMap, ";
            query += "Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs, MIN(Units.Price) AS MinPrice, MAX(Units.Price)AS MaxPrice ";
            query += "FROM Blocks INNER JOIN ";
            query += "Projects ON Blocks.ProjectId = Projects.ProjectId INNER JOIN ";
            query += "UnitTypes ON Blocks.BlockId = UnitTypes.BlockId INNER JOIN ";
            query += "Units ON UnitTypes.UnitTypeId = Units.UnitTypeId WHERE ";

            if (searchParams.townNames.Length > 0)
            {
                query += "( ";
                for (int i = 0; i < searchParams.townNames.Length; i++)
                {
                    query += "(Projects.TownName = '" + searchParams.townNames[i] + "') ";
                    if (i != searchParams.townNames.Length - 1)
                        query += "OR ";
                    else
                        query += ") AND ";
                }
            }

            if (searchParams.ethnicGroup == 'C')
                query += "(UnitTypes.QuotaChinese > 0) AND ";
            else if (searchParams.ethnicGroup == 'M')
                query += "(UnitTypes.QuotaMalay > 0) AND ";
            else if (searchParams.ethnicGroup == 'O')
                query += "(UnitTypes.QuotaOthers > 0) AND ";


            if (searchParams.unitTypes.Length > 0)
            {
                query += "( ";
                for (int i = 0; i < searchParams.unitTypes.Length; i++)
                {
                    query += "(UnitTypes.UnitTypeName = '" + searchParams.unitTypes[i] + "') ";
                    if (i != searchParams.unitTypes.Length - 1)
                        query += "OR ";
                    else
                        query += ") AND ";
                }
            }

            query += "(Units.Price >= " + searchParams.minPrice + ") AND (Units.Price <= " + searchParams.maxPrice + ") ";

            query += "GROUP BY Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, ";
            query += "Blocks.SitePlan, Blocks.TownMap, Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs ";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Block b = GetBlock(Convert.ToInt32(dr["BlockId"]));

                                using (UnitTypeController utc = new UnitTypeController())
                                {
                                    b.unitTypes = utc.GetUnitTypesInBlock(b.blockId);
                                    CalculateTravel(b, searchParams.postalCode);
                                }

                                //Block b = new Block();
                                //b.blockId = Convert.ToInt32(dr["BlockId"]);
                                //b.blockNo = dr["BlockNo"].ToString();
                                //b.street = dr["Street"].ToString();
                                //b.deliveryDate = Convert.ToDateTime(dr["DeliveryDate"]).ToShortDateString();
                                //b.locLat = Convert.ToDecimal(dr["LocLat"]);
                                //b.locLong = Convert.ToDecimal(dr["LocLong"]);
                                //b.sitePlan = dr["SitePlan"].ToString();
                                //b.townMap = dr["TownMap"].ToString();
                                //b.blockPlan = dr["BlockPlan"].ToString();
                                //b.unitDist = dr["UnitDist"].ToString();
                                //b.floorPlan = dr["FloorPlan"].ToString();
                                //b.layoutIdeas = dr["LayoutIdeas"].ToString();
                                //b.specs = dr["Specs"].ToString();
                                //b.minPrice = Convert.ToDecimal(dr["MinPrice"]);
                                //b.maxPrice = Convert.ToDecimal(dr["MaxPrice"]);
                                //b.project = new Project();
                                //b.project.projectId = dr["ProjectId"].ToString();

                                blocks.Add(b);
                            }
                        }
                    }
                }


                SortBlocks(searchParams.orderBy, blocks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return blocks;
        }

        [HttpGet]
        public Block GetBlockWithUnits(int blockId)
        {
            Block block = new Block();

            if (blockId == 0)
            {
                return block;
            }
            try
            {
                block = GetBlock(blockId);

                using (UnitTypeController utc = new UnitTypeController())
                {
                    block.unitTypes = utc.GetUnitTypesInBlock(block.blockId);
                }
                foreach (UnitType unitType in block.unitTypes)
                {
                    using (UnitController uc = new UnitController())
                    {
                        unitType.units = uc.GetUnitsInUnitType(unitType.unitTypeId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return block;
        }

        [HttpGet]
        public Block GetBlock(int blockId)
        {
            Block block = new Block();

            if (blockId == 0)
            {
                return block;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, Blocks.SitePlan, Blocks.TownMap, ";
                    query += "Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs, MIN(Units.Price) AS MinPrice, MAX(Units.Price)AS MaxPrice ";
                    query += "FROM Blocks INNER JOIN ";
                    query += "Projects ON Blocks.ProjectId = Projects.ProjectId INNER JOIN ";
                    query += "UnitTypes ON Blocks.BlockId = UnitTypes.BlockId INNER JOIN ";
                    query += "Units ON UnitTypes.UnitTypeId = Units.UnitTypeId WHERE Blocks.BlockId=@BlockId ";
                    query += "GROUP BY Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, ";
                    query += "Blocks.SitePlan, Blocks.TownMap, Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockId", blockId);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                block.blockId = Convert.ToInt32(dr["BlockId"]);
                                block.blockNo = dr["BlockNo"].ToString();
                                block.street = dr["Street"].ToString();
                                block.deliveryDate = Convert.ToDateTime(dr["DeliveryDate"]).ToShortDateString();
                                block.locLat = Convert.ToDecimal(dr["LocLat"]);
                                block.locLong = Convert.ToDecimal(dr["LocLong"]);
                                block.sitePlan = dr["SitePlan"].ToString();
                                block.townMap = dr["TownMap"].ToString();
                                block.blockPlan = dr["BlockPlan"].ToString();
                                block.unitDist = dr["UnitDist"].ToString();
                                block.floorPlan = dr["FloorPlan"].ToString();
                                block.layoutIdeas = dr["LayoutIdeas"].ToString();
                                block.specs = dr["Specs"].ToString();
                                block.minPrice = Convert.ToDecimal(dr["MinPrice"]);
                                block.maxPrice = Convert.ToDecimal(dr["MaxPrice"]);
                                block.project = new Project();
                                block.project.projectId = dr["ProjectId"].ToString();
                            }
                        }
                    }
                }

                using (ProjectController pc = new ProjectController())
                {
                    block.project = pc.GetProject(block.project.projectId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return block;
        }



        private void SortBlocks(char orderBy, List<Block> blocks)
        {
            if (orderBy == 'T')
            {
                SortBlocksByPrice(blocks);
                SortBlocksByTime(blocks);
            }
            else if (orderBy == 'P')
            {
                SortBlocksByTime(blocks);
                SortBlocksByPrice(blocks);
            }
        }

        private void SortBlocksByPrice(List<Block> blocks)
        {
            Block temp = new Block();

            for (int write = 0; write < blocks.Count(); write++)
            {
                for (int sort = 0; sort < blocks.Count() - 1; sort++)
                {
                    if (blocks[sort].maxPrice > blocks[sort + 1].maxPrice)
                    {
                        temp = blocks[sort + 1];
                        blocks[sort + 1] = blocks[sort];
                        blocks[sort] = temp;
                    }
                }
            }

            for (int write = 0; write < blocks.Count(); write++)
            {
                for (int sort = 0; sort < blocks.Count() - 1; sort++)
                {
                    if (blocks[sort].minPrice > blocks[sort + 1].minPrice)
                    {
                        temp = blocks[sort + 1];
                        blocks[sort + 1] = blocks[sort];
                        blocks[sort] = temp;
                    }
                }
            }
        }

        private void SortBlocksByTime(List<Block> blocks)
        {
            Block temp = new Block();

            for (int write = 0; write < blocks.Count(); write++)
            {
                for (int sort = 0; sort < blocks.Count() - 1; sort++)
                {
                    if (blocks[sort].travelTime > blocks[sort + 1].travelTime)
                    {
                        temp = blocks[sort + 1];
                        blocks[sort + 1] = blocks[sort];
                        blocks[sort] = temp;
                    }
                }
            }
        }



        private void CalculateTravel(Block block, string postalCode)
        {
            if (postalCode.Equals("")) return;
            try
            {
                string uri = "https://maps.googleapis.com/maps/api/distancematrix/xml?units=metric";
                string key = "AIzaSyAx2cHrI8CjdzkiByY_FS1nV93CFx9LD54";
                uri += "&key=" + key + "&origins=" + block.locLat + "," + block.locLong + "&destinations=" + postalCode;

                XmlTextReader reader = new XmlTextReader(uri);
                reader.ReadToFollowing("duration");
                reader.ReadToFollowing("value");
                block.travelTime = Convert.ToInt32(reader.ReadElementContentAsString());

                reader.ReadToFollowing("distance");
                reader.ReadToFollowing("value");
                block.travelDist = Convert.ToInt32(reader.ReadElementContentAsString());
            }
            catch (Exception ex)
            {
            }
        }


        [HttpPost]
        public int AddBlock(Block block)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM Blocks WHERE BlockNo=@BlockNo AND Street=@Street";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockNo", block.blockNo);
                        cmd.Parameters.AddWithValue("@Street", block.street);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                return Convert.ToInt32(dr["BlockId"]);
                            }
                        }
                    }
                }


                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "INSERT INTO Blocks(BlockNo,Street,ProjectId,DeliveryDate,LocLat,LocLong,SitePlan,TownMap,BlockPlan,UnitDist,FloorPlan,LayoutIdeas,Specs) VALUES(@BlockNo,@Street,@ProjectId,@DeliveryDate,@LocLat,@LocLong,@SitePlan,@TownMap,@BlockPlan,@UnitDist,@FloorPlan,@LayoutIdeas,@Specs);";
                    query += "SELECT CAST(scope_identity() AS int)";
                    using (ProjectController pc = new ProjectController())
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BlockNo", block.blockNo);
                        cmd.Parameters.AddWithValue("@Street", block.street);
                        cmd.Parameters.AddWithValue("@ProjectId", pc.AddProject(block.project));
                        cmd.Parameters.AddWithValue("@DeliveryDate", block.deliveryDate);
                        cmd.Parameters.AddWithValue("@LocLat", block.locLat);
                        cmd.Parameters.AddWithValue("@LocLong", block.locLong);
                        cmd.Parameters.AddWithValue("@SitePlan", block.sitePlan);
                        cmd.Parameters.AddWithValue("@TownMap", block.townMap);
                        cmd.Parameters.AddWithValue("@BlockPlan", block.blockPlan);
                        cmd.Parameters.AddWithValue("@UnitDist", block.unitDist);
                        cmd.Parameters.AddWithValue("@FloorPlan", block.floorPlan);
                        cmd.Parameters.AddWithValue("@LayoutIdeas", block.layoutIdeas);
                        cmd.Parameters.AddWithValue("@Specs", block.specs);
                        conn.Open();

                        return (Int32)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }
}
