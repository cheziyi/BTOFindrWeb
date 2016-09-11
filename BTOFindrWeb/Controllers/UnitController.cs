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
    public class UnitController : ApiController
    {
        String connString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        [HttpGet]
        public Unit GetUnit(int unitId)
        {
            Unit unit = new Unit();

            if (unitId == 0)
            {
                return unit;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM Units WHERE UnitId = @UnitId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitId", unitId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                unit.unitId = Convert.ToInt32(dr["UnitId"]);
                                unit.unitNo = dr["UnitNo"].ToString();
                                unit.price = Convert.ToDecimal(dr["Price"]);
                                unit.floorArea = Convert.ToInt32(dr["FloorArea"]);
                                unit.avail = Convert.ToBoolean(dr["Avail"]);
                                unit.faveCount = Convert.ToInt32(dr["FaveCount"]);
                                unit.unitType = new UnitType();
                                unit.unitType.unitTypeId = Convert.ToInt32(dr["UnitTypeId"]);
                            }
                        }
                    }
                }

                using (UnitTypeController utc = new UnitTypeController())
                {
                    unit.unitType = utc.GetUnitType(unit.unitType.unitTypeId);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return unit;
        }



        [HttpGet]
        public List<Unit> GetUnitsInUnitType(int unitTypeId)
        {
            List<Unit> units = new List<Unit>();

            if (unitTypeId == 0)
            {
                return units;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "SELECT * FROM Units WHERE UnitTypeId=@UnitTypeId ORDER BY UnitNo ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitTypeId", unitTypeId);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Unit u = new Unit();
                                u.unitId = Convert.ToInt32(dr["UnitId"]);
                                u.unitNo = dr["UnitNo"].ToString();
                                u.price = Convert.ToDecimal(dr["Price"]);
                                u.floorArea = Convert.ToInt32(dr["FloorArea"]);
                                u.avail = Convert.ToBoolean(dr["Avail"]);
                                u.faveCount = Convert.ToInt32(dr["FaveCount"]);
                                units.Add(u);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return units;
        }




    }
}
