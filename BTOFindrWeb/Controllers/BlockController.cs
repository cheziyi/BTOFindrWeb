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
    /// <summary>
    /// BlockController web service class for retrieving and adding of blocks in database.
    /// 
    /// Author: Calvin Che Zi Yi
    /// </summary>
    public class BlockController : ApiController
    {
        /// <summary>
        /// SQL connection string from web.config
        /// </summary>
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        /// <summary>
        /// Method to get particular blocks using the search parameters.
        /// </summary>
        /// <param name="searchParams">A SearchParamter object that contains the values to be used for retrieval</param>
        /// <returns>A list of Block objects with relevant information</returns>
        [HttpPost]
        public List<Block> SearchBlocks(SearchParameters searchParams)
        {
            // Initialize a list of Block object
            List<Block> blocks = new List<Block>();

            // Query the database for blocks that fulfill the search paramters
            String query = "SELECT Blocks.BlockId, Blocks.BlockNo, Blocks.Street, Blocks.ProjectId, Blocks.DeliveryDate, Blocks.LocLat, Blocks.LocLong, Blocks.SitePlan, Blocks.TownMap, ";
            query += "Blocks.BlockPlan, Blocks.UnitDist, Blocks.FloorPlan, Blocks.LayoutIdeas, Blocks.Specs, MIN(Units.Price) AS MinPrice, MAX(Units.Price)AS MaxPrice ";
            query += "FROM Blocks INNER JOIN ";
            query += "Projects ON Blocks.ProjectId = Projects.ProjectId INNER JOIN ";
            query += "UnitTypes ON Blocks.BlockId = UnitTypes.BlockId INNER JOIN ";
            query += "Units ON UnitTypes.UnitTypeId = Units.UnitTypeId WHERE ";

            // If search paramter includes town names
            if (searchParams.townNames.Length > 0)
            {
                query += "( ";
                // Loop through the town names
                for (int i = 0; i < searchParams.townNames.Length; i++)
                {
                    // Add town name into the query
                    query += "(Projects.TownName = '" + searchParams.townNames[i] + "') ";
                    if (i != searchParams.townNames.Length - 1)
                        // If not the last town name
                        query += "OR ";
                    else
                        // If reached the last town name
                        query += ") AND ";
                }
            }

            // If search paramter includes ethnic group, add relevant ethnic group into the query
            // 'C' = Chinese, 'M' = Malay, 'O' = Indian/Others
            if (searchParams.ethnicGroup == 'C')
                query += "(UnitTypes.QuotaChinese > 0) AND ";
            else if (searchParams.ethnicGroup == 'M')
                query += "(UnitTypes.QuotaMalay > 0) AND ";
            else if (searchParams.ethnicGroup == 'O')
                query += "(UnitTypes.QuotaOthers > 0) AND ";

            // If search paramter includes unit types
            if (searchParams.unitTypes.Length > 0)
            {
                query += "( ";
                // Loop through the unit types
                for (int i = 0; i < searchParams.unitTypes.Length; i++)
                {
                    // Add unit type into the query
                    query += "(UnitTypes.UnitTypeName = '" + searchParams.unitTypes[i] + "') ";
                    if (i != searchParams.unitTypes.Length - 1)
                        // If not the last unit type
                        query += "OR ";
                    else
                        // If reached the last unit type
                        query += ") AND ";
                }
            }

            // Add search for price within search parameter price range into the query
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
                                // If a row is returned, initialize a Block object
                                Block b = GetBlock(Convert.ToInt32(dr["BlockId"]));

                                using (UnitTypeController utc = new UnitTypeController())
                                {
                                    b.unitTypes = utc.GetUnitTypesInBlock(b.blockId);
                                    CalculateTravel(b, searchParams.postalCode);
                                }

                                // Add Block object into the list of blocks
                                blocks.Add(b);
                            }
                        }
                    }
                }
                
                SortBlocks(searchParams.orderBy, blocks);
            }
            catch (Exception ex)
            {
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return the list of Block objects
            return blocks;
        }

        /// <summary>
        /// Method to get information of block and units within the block.
        /// </summary>
        /// <param name="blockId">The id of a block</param>
        /// <returns>A Block object with relevant unit and unit types information</returns>
        [HttpGet]
        public Block GetBlockWithUnits(int blockId)
        {
            // Initialize a Block object
            Block block = new Block();

            if (blockId == 0)
            {
                // Return empty block if id is 0
                return block;
            }
            try
            {
                // Get block information
                block = GetBlock(blockId);

                using (UnitTypeController utc = new UnitTypeController())
                {
                    // Get unit types in block
                    block.unitTypes = utc.GetUnitTypesInBlock(block.blockId);
                }
                foreach (UnitType unitType in block.unitTypes)
                {
                    using (UnitController uc = new UnitController())
                    {
                        // Get all units in each unit types
                        unitType.units = uc.GetUnitsInUnitType(unitType.unitTypeId);
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return the Block object
            return block;
        }

        /// <summary>
        /// Method to get block information.
        /// </summary>
        /// <param name="blockId">The id of a block</param>
        /// <returns>A Block object with relevant information</returns>
        [HttpGet]
        public Block GetBlock(int blockId)
        {
            // Initialize a Block object
            Block block = new Block();

            if (blockId == 0)
            {
                // Return empty block if id is 0
                return block;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for block with the specific block id
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
                                // If a row is returned, set all block information respectively
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
                // If an error occurred, write error message on console
                Console.WriteLine(ex.Message);
            }
            // Return the Block object
            return block;
        }

        /// <summary>
        /// Method to sort the list of blocks accordingly
        /// </summary>
        /// <param name="orderBy">A char to indicate the sorting order</param>
        /// <param name="blocks">A list of Block objects for sorting</param>
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

        /// <summary>
        /// Method to sort the list of blocks according to price
        /// </summary>
        /// <param name="blocks">A list of Block objects for sorting</param>
        private void SortBlocksByPrice(List<Block> blocks)
        {
            // Initialize a Block object for temporary storage
            Block temp = new Block();

            for (int write = 0; write < blocks.Count(); write++)
            {
                for (int sort = 0; sort < blocks.Count() - 1; sort++)
                {
                    if (blocks[sort].maxPrice > blocks[sort + 1].maxPrice)
                    {
                        // If the current block has a higher maximum price,
                        // swap the position of the current block with the next block in the list of blocks
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
                        // If the current block has a higher minimum price,
                        // swap the position of the current block with the next block in the list of blocks
                        temp = blocks[sort + 1];
                        blocks[sort + 1] = blocks[sort];
                        blocks[sort] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Method to sort the list of blocks according to travel time
        /// </summary>
        /// <param name="blocks">A list of Block objects for sorting</param>
        private void SortBlocksByTime(List<Block> blocks)
        {
            // Initialize a Block object for temporary storage
            Block temp = new Block();

            for (int write = 0; write < blocks.Count(); write++)
            {
                for (int sort = 0; sort < blocks.Count() - 1; sort++)
                {
                    if (blocks[sort].travelTime > blocks[sort + 1].travelTime)
                    {
                        // If the current block takes a longer travelling time,
                        // swap the position of the current block with the next block in the list of blocks
                        temp = blocks[sort + 1];
                        blocks[sort + 1] = blocks[sort];
                        blocks[sort] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Method to find the travelling time between the block and postal code location
        /// </summary>
        /// <param name="block">A Block object</param>
        /// <param name="postalCode">A string that contains the postal code</param>
        private void CalculateTravel(Block block, string postalCode)
        {
            // If postal code is empty, return
            if (postalCode.Equals("")) return;
            try
            {
                // Form the url to get information from Google Maps API
                string uri = "https://maps.googleapis.com/maps/api/distancematrix/xml?units=metric";
                string key = "AIzaSyAx2cHrI8CjdzkiByY_FS1nV93CFx9LD54";
                uri += "&key=" + key + "&origins=" + block.locLat + "," + block.locLong + "&destinations=Singapore " + postalCode;

                XmlTextReader reader = new XmlTextReader(uri);

                // Get duration information and set into block
                reader.ReadToFollowing("duration");
                reader.ReadToFollowing("value");
                block.travelTime = Convert.ToInt32(reader.ReadElementContentAsString());

                // Get distance information and set into block
                reader.ReadToFollowing("distance");
                reader.ReadToFollowing("value");
                block.travelDist = Convert.ToInt32(reader.ReadElementContentAsString());
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Method to add block into database. If it exists in the database, return its block id.
        /// </summary>
        /// <param name="block">A Block object to be added into the database</param>
        /// <returns>The respective block id for the inserted block</returns>
        [HttpPost]
        public int AddBlock(Block block)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query the database for any existing block with the same block number and street
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
                                // If a row is returned, return the retrieved block id
                                return Convert.ToInt32(dr["BlockId"]);
                            }
                        }
                    }
                }


                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Query to insert block into database
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

                        // Return the block id
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
    }
}
