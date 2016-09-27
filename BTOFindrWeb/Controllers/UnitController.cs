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

        [HttpPost]
        public IEnumerable<Unit> GetRecommendedUnits(int[] unitIds)
        {
            List<Unit> units = new List<Unit>();

            return units;
        }

        [HttpPost]
        public Unit GetUnitWithPayables(PayableParameters payParams)
        {
            Unit unit = GetUnit(payParams.unitId);
            FeesPayable fees = new FeesPayable();

            if(payParams.income<=1500)
            {
                fees.grantAmt= 80000;
            }
            else if (payParams.income <= 2000)
            {
                fees.grantAmt = 75000;
            }
            else if (payParams.income <= 2500)
            {
                fees.grantAmt = 70000;
            }
            else if (payParams.income <= 3000)
            {
                fees.grantAmt = 65000;
            }
            else if (payParams.income <= 3500)
            {
                fees.grantAmt = 60000;
            }
            else if (payParams.income <= 4000)
            {
                fees.grantAmt = 55000;
            }
            else if (payParams.income <= 4500)
            {
                fees.grantAmt = 50000;
            }
            else if (payParams.income <= 5000)
            {
                fees.grantAmt = 45000;
            }
            else if (payParams.income <= 5500)
            {
                fees.grantAmt = 35000;
            }
            else if (payParams.income <= 6000)
            {
                fees.grantAmt = 30000;
            }
            else if (payParams.income <= 6500)
            {
                fees.grantAmt = 25000;
            }
            else if (payParams.income <= 7000)
            {
                fees.grantAmt = 20000;
            }
            else if (payParams.income <= 7500)
            {
                fees.grantAmt = 15000;
            }
            else if (payParams.income <= 8000)
            {
                fees.grantAmt = 10000;
            }
            else if (payParams.income <= 8500)
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

            //if (payParams.loan == 'H')
            //{
            decimal downpayment = Decimal.Multiply(fees.afterGrantAmt, (decimal)0.10);
            //}
            //else if(payParams.loan == 'B')
            //{
            //    if(payParams.loanPercent >60 && payParams.loanPercent <= 80)
            //    {
            //        fees.signingFeesCash = Decimal.Multiply(fees.afterGrantAmt, (decimal)0.05);
            //        fees.signingFeesCpf = Decimal.Multiply(fees.afterGrantAmt, (decimal)0.15);
            //    }
            //    else if (payParams.loanPercent > 50 && payParams.loanPercent <= 60)
            //    {
            //        fees.signingFeesCash = Decimal.Multiply(fees.afterGrantAmt, (decimal)0.10);
            //        fees.signingFeesCpf = Decimal.Multiply(fees.afterGrantAmt, (decimal)0.30);
            //    }
            //    else
            //    {

            //    }

            //}

            fees.signingFeesCpf += downpayment;

            decimal stampDutyLease = 0;

            if (fees.afterGrantAmt <= 180000)
            {
                stampDutyLease += Decimal.Multiply(fees.afterGrantAmt, (decimal)0.01);
            }
            else
            {
                stampDutyLease += Decimal.Multiply(180000, (decimal)0.01);
                if (fees.afterGrantAmt <= 360000)
                {
                    stampDutyLease += Decimal.Multiply((fees.afterGrantAmt - 180000), (decimal)0.02);
                }
                else
                {
                    stampDutyLease += Decimal.Multiply(180000, (decimal)0.02);
                    stampDutyLease += Decimal.Multiply((fees.afterGrantAmt - 360000), (decimal)0.03);
                }
            }
            stampDutyLease = Math.Round(Decimal.Multiply(stampDutyLease, (decimal)100)) / 100;
            fees.signingFeesCpf += stampDutyLease;


            decimal conveyancing = 0;

            if (fees.afterGrantAmt <= 30000)
            {
                conveyancing += Decimal.Multiply(Math.Round(fees.afterGrantAmt / 1000), (decimal)0.90);
            }
            else
            {
                conveyancing += Decimal.Multiply(Math.Round((decimal)30000 / (decimal)1000), (decimal)0.90);
                if (fees.afterGrantAmt <= 60000)
                {
                    conveyancing += Decimal.Multiply(Math.Round((fees.afterGrantAmt - 30000) / 1000), (decimal)0.72);
                }
                else
                {
                    conveyancing += Decimal.Multiply(Math.Round((decimal)30000 / (decimal)1000), (decimal)0.72);
                    conveyancing += Decimal.Multiply(Math.Round((fees.afterGrantAmt - 60000) / 1000), (decimal)0.60);
                }
            }

            if (conveyancing < 20) conveyancing = 20;
            conveyancing = Decimal.Multiply(conveyancing, (decimal)1.07);
            conveyancing = Math.Round(Decimal.Multiply(conveyancing, (decimal)100)) / 100;

            fees.signingFeesCpf += conveyancing;

            fees.signingFeesCpf += (decimal)64.45;

            fees.signingFeesCpf -= fees.optionFee;

            if (payParams.currentCpf < fees.signingFeesCpf)
            {
                fees.signingFeesCash = fees.signingFeesCpf - payParams.currentCpf;
                fees.signingFeesCpf = payParams.currentCpf;
                payParams.currentCpf = 0;
            }
            else
            {
                payParams.currentCpf -= fees.signingFeesCpf;
            }

            fees.collectionFeesCash = 0;
            fees.collectionFeesCpf = 0;

            fees.collectionFeesCpf += (decimal)38.30;

            decimal survey = 0;

            if (unit.unitType.unitTypeName.Equals("3-Room ($6k ceiling)"))
            {
                survey = (decimal)212.50;
            }
            else if (unit.unitType.unitTypeName.Equals("3-Room"))
            {
                survey = (decimal)212.50;
            }
            else if (unit.unitType.unitTypeName.Equals("4-Room"))
            {
                survey = (decimal)275;
            }
            else if (unit.unitType.unitTypeName.Equals("5-Room"))
            {
                survey = (decimal)325;
            }
            else if (unit.unitType.unitTypeName.Equals("5-Room/3Gen"))
            {
                survey = (decimal)325;
            }

            survey = Decimal.Multiply(survey, (decimal)1.07);
            fees.collectionFeesCpf += survey;

            if (payParams.currentCpf < fees.collectionFeesCpf)
            {
                fees.collectionFeesCash = fees.collectionFeesCpf - payParams.currentCpf;
                fees.collectionFeesCpf = payParams.currentCpf;
            }



            decimal balance = fees.afterGrantAmt - downpayment;

            fees.monthlyCpf = balance / (payParams.loanTenure * 12);
            fees.monthlyCash = 0;

            if (payParams.monthlyCpf < fees.monthlyCpf)
            {
                fees.monthlyCash = fees.monthlyCpf - payParams.monthlyCpf;
                fees.monthlyCpf = payParams.monthlyCpf;
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
