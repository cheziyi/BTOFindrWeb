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

        [HttpPost]
        public List<Unit> GetRecommendedUnits(int[] unitIds)
        {
            List<Unit> units = new List<Unit>();


            String query = "SELECT TOP (20) Units.UnitId ";
            query += "FROM Units INNER JOIN ";
            query += "UnitTypes ON UnitTypes.UnitTypeId = Units.UnitTypeId INNER JOIN ";
            query += "Blocks ON Blocks.BlockId = UnitTypes.BlockId INNER JOIN ";
            query += "Projects ON Blocks.ProjectId = Projects.ProjectId WHERE ";


            String priceQuery = "( ";
            String townQuery = "( ";
            String unitQuery = "( ";

            for (int i = 0; i < unitIds.Length; i++)
            {
                Unit u = GetUnit(unitIds[i]);

                priceQuery += "((Units.Price >= " + Math.Round(u.price / 10000) * 10000 + ") AND (Units.Price <= " + Math.Round((u.price+10000) / 10000) * 10000 + ")) ";
                if (i != unitIds.Length - 1)
                    priceQuery += "OR ";
                else
                    priceQuery += ") AND ";

                townQuery += "(Projects.TownName = '" + u.unitType.block.project.townName + "') ";
                if (i != unitIds.Length - 1)
                    townQuery += "OR ";
                else
                    townQuery += ") AND ";

                unitQuery += "(Units.UnitId != " + u.unitId + ") ";
                if (i != unitIds.Length - 1)
                    unitQuery += "AND ";
                else
                    unitQuery += ") ";
            }



            query += priceQuery + townQuery + unitQuery;
            query += "ORDER BY Units.FaveCount DESC, Units.Price ASC";



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
                                Unit u = GetUnit(Convert.ToInt32(dr["UnitId"]));
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

        [HttpPost]
        public Unit GetUnitWithPayables(Profile profile, int unitId)
        {
            Unit unit = GetUnit(unitId);
            FeesPayable fees = new FeesPayable();

            if (profile.income <= 1500)
            {
                fees.grantAmt = 80000;
            }
            else if (profile.income <= 2000)
            {
                fees.grantAmt = 75000;
            }
            else if (profile.income <= 2500)
            {
                fees.grantAmt = 70000;
            }
            else if (profile.income <= 3000)
            {
                fees.grantAmt = 65000;
            }
            else if (profile.income <= 3500)
            {
                fees.grantAmt = 60000;
            }
            else if (profile.income <= 4000)
            {
                fees.grantAmt = 55000;
            }
            else if (profile.income <= 4500)
            {
                fees.grantAmt = 50000;
            }
            else if (profile.income <= 5000)
            {
                fees.grantAmt = 45000;
            }
            else if (profile.income <= 5500)
            {
                fees.grantAmt = 35000;
            }
            else if (profile.income <= 6000)
            {
                fees.grantAmt = 30000;
            }
            else if (profile.income <= 6500)
            {
                fees.grantAmt = 25000;
            }
            else if (profile.income <= 7000)
            {
                fees.grantAmt = 20000;
            }
            else if (profile.income <= 7500)
            {
                fees.grantAmt = 15000;
            }
            else if (profile.income <= 8000)
            {
                fees.grantAmt = 10000;
            }
            else if (profile.income <= 8500)
            {
                fees.grantAmt = 5000;
            }

            fees.afterGrantAmt = unit.price - fees.grantAmt;

            fees.applFee = 10;

            if (unit.unitType.unitTypeName.Equals("3-Room ($6k ceiling)"))
            {
                fees.optionFee = 1000;
            }
            else if (unit.unitType.unitTypeName.Equals("3-Room"))
            {
                fees.optionFee = 1000;
            }
            else if (unit.unitType.unitTypeName.Equals("4-Room"))
            {
                fees.optionFee = 2000;
            }
            else if (unit.unitType.unitTypeName.Equals("5-Room"))
            {
                fees.optionFee = 2000;
            }
            else if (unit.unitType.unitTypeName.Equals("5-Room/3Gen"))
            {
                fees.optionFee = 2000;
            }

            fees.signingFeesCash = 0;
            fees.signingFeesCpf = 0;

            decimal downpayment = fees.afterGrantAmt * 0.10m;

            fees.signingFeesCpf += downpayment;

            decimal stampDutyLease = 0;

            if (fees.afterGrantAmt <= 180000)
            {
                stampDutyLease += fees.afterGrantAmt * 0.01m;
            }
            else
            {
                stampDutyLease += 180000 * 0.01m;
                if (fees.afterGrantAmt <= 360000)
                {
                    stampDutyLease += (fees.afterGrantAmt - 180000m) * 0.02m;
                }
                else
                {
                    stampDutyLease += 180000 * 0.02m;
                    stampDutyLease += (fees.afterGrantAmt - 360000) * 0.03m;
                }
            }
            stampDutyLease = Math.Round(stampDutyLease * 100m) / 100;
            fees.signingFeesCpf += stampDutyLease;


            decimal conveyancing = 0;

            if (fees.afterGrantAmt <= 30000)
            {
                conveyancing += Math.Round(fees.afterGrantAmt / 1000) * 0.90m;
            }
            else
            {
                conveyancing += Math.Round(30000m / 1000m) * 0.90m;
                if (fees.afterGrantAmt <= 60000)
                {
                    conveyancing += Math.Round((fees.afterGrantAmt - 30000) / 1000) * 0.72m;
                }
                else
                {
                    conveyancing += Math.Round(30000m / 1000m) * 0.72m;
                    conveyancing += Math.Round((fees.afterGrantAmt - 60000) / 1000) * 0.60m;
                }
            }

            if (conveyancing < 20) conveyancing = 20;
            conveyancing = conveyancing * 1.07m;
            conveyancing = Math.Round(conveyancing * 100m) / 100;

            fees.signingFeesCpf += conveyancing;

            fees.signingFeesCpf += 64.45m;

            fees.signingFeesCpf -= fees.optionFee;

            if (profile.currentCpf < fees.signingFeesCpf)
            {
                fees.signingFeesCash = fees.signingFeesCpf - profile.currentCpf;
                fees.signingFeesCpf = profile.currentCpf;
                profile.currentCpf = 0;
            }
            else
            {
                profile.currentCpf -= fees.signingFeesCpf;
            }

            fees.collectionFeesCash = 0;
            fees.collectionFeesCpf = 0;

            fees.collectionFeesCpf += 38.30m;

            decimal survey = 0;

            if (unit.unitType.unitTypeName.Equals("3-Room ($6k ceiling)"))
            {
                survey = 212.50m;
            }
            else if (unit.unitType.unitTypeName.Equals("3-Room"))
            {
                survey = 212.50m;
            }
            else if (unit.unitType.unitTypeName.Equals("4-Room"))
            {
                survey = 275m;
            }
            else if (unit.unitType.unitTypeName.Equals("5-Room"))
            {
                survey = 325m;
            }
            else if (unit.unitType.unitTypeName.Equals("5-Room/3Gen"))
            {
                survey = 325m;
            }

            survey = survey * 1.07m;
            fees.collectionFeesCpf += survey;

            if (profile.currentCpf < fees.collectionFeesCpf)
            {
                fees.collectionFeesCash = fees.collectionFeesCpf - profile.currentCpf;
                fees.collectionFeesCpf = profile.currentCpf;
            }


            decimal balance = fees.afterGrantAmt - downpayment;

            fees.monthlyCpf = balance / (profile.loanTenure * 12);
            fees.monthlyCash = 0;

            if (profile.monthlyCpf < fees.monthlyCpf)
            {
                fees.monthlyCash = fees.monthlyCpf - profile.monthlyCpf;
                fees.monthlyCpf = profile.monthlyCpf;
            }

            unit.fees = fees;
            return unit;
        }


        [HttpGet]
        public void AddFaveUnit(int unitId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "UPDATE Units SET FaveCount=(FaveCount+1) WHERE UnitId=@UnitId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitId", unitId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [HttpGet]
        public void RemoveFaveUnit(int unitId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    String query = "UPDATE Units SET FaveCount=(FaveCount-1) WHERE UnitId=@UnitId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UnitId", unitId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
