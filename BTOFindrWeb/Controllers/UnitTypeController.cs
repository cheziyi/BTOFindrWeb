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
    public class UnitTypeController : ApiController
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
    }
}
