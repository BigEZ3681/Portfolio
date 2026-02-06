using ShoeSensation.Framework.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeBuck_Sender.HelperObjects
{
    public class ShoeBuckCouponCode
    {
        public string DiscountCode;
        public string Description;
        public decimal AmountOff;
        public string DisplayHelper;
        public string CouponCode;
        public string CouponType;
        public int NumberOfDaysValid;
        public bool CampaignFound;
        public bool CouponCodeGenerated;

        public ShoeBuckCouponCode()
        {
            DiscountCode = "";
            Description = "";
            AmountOff = 0M;
            DisplayHelper = "";
            CouponCode = "";
            CouponType = "SSI BUCKS";
            NumberOfDaysValid = 0;
            CampaignFound = false;
            CouponCodeGenerated = false;
        }

        /// <summary>
        /// Helper method to return a DIV string display on LisTrack Transactonal Email.
        /// </summary>
        /// <returns></returns>
        public string ToListrackTransationalDiv(string _orderNumber)
        {
            string DivDisplayHelper = string.Format("Thank You For Your Recent Order {1}!  Congratulations, You've Earned ${0} In Shoe Sensation Shoe Bucks To Use On Your Next In Store Visit!", AmountOff, _orderNumber);

            ///ChrisFLAG: In case we ever want to do a Percent off, here is some holder text.
            //if (DisplayHelper == "% off")
            //{
            //    DivDisplayHelper = string.Format("Congratulations, You've Earned {0}% Off Your Next Shoe Sensation Purchase!", AmountOff);
            //}

            return string.Format(@"<div>
                                        <table width = '100%' border = '0' cellspacing = '0' cellpadding = '0'>
                                            <tr>
                                                <td align = 'center'>
                                                    <h2 style = 'font-family: sans-serif;'> {0} </h2 >
                                                </td ></tr></table><table width = '100%' border = '0' cellspacing = '0' cellpadding = '0' >
                                            </tr >
                                        </table>
                                    </div>"
                            , DivDisplayHelper
                            );
        }
    }

    /// <summary>
    /// Creates serialized coupon to send to Imagine, Magento, and Klaviyo
    /// </summary>
    public static class ShoeBucksHelper
    {
        /// <summary>
        /// Find an eligable campaign for amount and storenumber passed.
        /// </summary>
        /// <param name="TotalSpent">Total amount spent on ticket</param>
        /// <param name="StoreNumber">Store number to be used for campaign search (not implemented yet)</param>
        /// <returns>Campaign with data returned. Check value CampaignFound. If True then all is well.</returns>
        private static ShoeBuckCouponCode FindEligibleCampaignForAmountStore(decimal TotalSpent, string StoreNumber)
        {
            //Need to Test from IMAGINE Db function. Once we do that we can skip the HARD Coded one.
            //return FindEligibleCampaignForAmountStore_FromImagineDB(TotalSpent, StoreNumber);
            return FindEligibleCampaignForAmountStore_HardCoded(TotalSpent, StoreNumber);
        }

        private static ShoeBuckCouponCode FindEligibleCampaignForAmountStore_FromImagineDB(decimal TotalSpent, string StoreNumber)
        {
            //Chris, this process should be very similar to Imagine POS's way of determine which campaign to use.

            ShoeBuckCouponCode CampaignToReturn = new ShoeBuckCouponCode();

            ImagineDataAccess imagineDal = new ImagineDataAccess();
            //imagineDal.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            imagineDal.QueryToRun = @"select top 1 d.Amount as Amount
	                                    ,	case d.DiscountType
			                                    when 'A' then '$ off'
			                                    when 'F' then 'Price Set To'
			                                    when 'P' then '% off'
			                                    else d.DiscountType end as DiscountType
	                                    ,	s.Description as Description
	                                    ,	s.DiscountCode as DiscountCode
	                                    ,	s.NumberOfDaysValid
	                                    from StoreCampaigns as s
	                                    inner join Discounts as d on s.DiscountCode = d.DiscountCode
	                                    where StoreNumber = @StoreNumber
		                                    and DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) between s.StartDate and s.StopDate
		                                    and s.Status = 'Y'
		                                    --and s.DiscountCode like 'SHOEBUCK%' 
		                                    and @TotalSpent between TicketMin and TicketMax
		                                    and d.DiscountType <> 'F' --Exclude the 'Set To' Discounts for now.
                                        --Use the order by here to try to privilege the customer.
	                                    Order by case d.DiscountType
			                                    when 'A' then 2
			                                    when 'F' then 0
			                                    when 'P' then 1
			                                    else 4 end asc
		                                    , NumberOfDaysValid desc";


            SqlParameter TotalSpentParam = new SqlParameter("@TotalSpent", SqlDbType.Money);
            SqlParameter StoreNumberParam = new SqlParameter("@StoreNumber", SqlDbType.Money);
            TotalSpentParam.Value = TotalSpent;
            StoreNumberParam.Value = StoreNumber;
            imagineDal.AddSqlParameter(TotalSpentParam);
            imagineDal.AddSqlParameter(StoreNumberParam);

            DataSet results = imagineDal.returnQueryResultsDataSet();
            if (results.Tables.Count > 0)
            {
                if (results.Tables[0].Rows.Count > 0)
                {
                    //For now we are just returning the first row we find. May need to add some more specific filters later.

                    int _numOfDaysValid = 0;
                    decimal _amountOff = 0;

                    int.TryParse(results.Tables[0].Rows[0]["NumberOfDaysValid"].ToString(), out _numOfDaysValid);
                    decimal.TryParse(results.Tables[0].Rows[0]["Amount"].ToString(), out _amountOff);

                    CampaignToReturn.DiscountCode = results.Tables[0].Rows[0]["DiscountCode"].ToString();
                    CampaignToReturn.Description = results.Tables[0].Rows[0]["Description"].ToString();
                    CampaignToReturn.CouponType = "SSI BUCKS";
                    CampaignToReturn.AmountOff = _amountOff;
                    CampaignToReturn.DisplayHelper = results.Tables[0].Rows[0]["DiscountType"].ToString();
                    CampaignToReturn.NumberOfDaysValid = _numOfDaysValid;
                    CampaignToReturn.CampaignFound = true;
                }
            }
            return CampaignToReturn;
        }

        private static ShoeBuckCouponCode FindEligibleCampaignForAmountStore_HardCoded(decimal TotalSpent, string StoreNumber)
        {
            ShoeBuckCouponCode CampaignToReturn = new ShoeBuckCouponCode();

            switch (TotalSpent)
            {
                case var n when n > 99.99M:
                    CampaignToReturn.DiscountCode = "SHOEBUCK20";
                    CampaignToReturn.Description = "Shoe Bucks 20";
                    CampaignToReturn.AmountOff = 20;
                    CampaignToReturn.NumberOfDaysValid = 10;
                    CampaignToReturn.CampaignFound = true;
                    break;
                case var n when n > 74.99M:
                    CampaignToReturn.DiscountCode = "SHOEBUCK15";
                    CampaignToReturn.Description = "Shoe Bucks 15";
                    CampaignToReturn.AmountOff = 15;
                    CampaignToReturn.NumberOfDaysValid = 10;
                    CampaignToReturn.CampaignFound = true;
                    break;
                case var n when n > 49.99M:
                    CampaignToReturn.DiscountCode = "SHOEBUCK10";
                    CampaignToReturn.Description = "Shoe Bucks 10";
                    CampaignToReturn.AmountOff = 10;
                    CampaignToReturn.NumberOfDaysValid = 10;
                    CampaignToReturn.CampaignFound = true;
                    break;
                case var n when n > 24.99M:
                    CampaignToReturn.DiscountCode = "SHOEBUCK5";
                    CampaignToReturn.Description = "Shoe Bucks 5";
                    CampaignToReturn.AmountOff = 5;
                    CampaignToReturn.NumberOfDaysValid = 10;
                    CampaignToReturn.CampaignFound = true;
                    break;
            }

            return CampaignToReturn;
        }

        /// <summary>
        /// Searches the Imagine CouponCodes Table and grabs the next unused coupon code for insert.
        /// CouponCode = StoreNumber + OrderNumber + 4 digit - Zero Padded - RandomNumber between 1 and 9999. (Character Limit 30)
        /// Imagine's POS returns StoreNumber + Ticket + 4 digit - Zero Padded - Zero Padded RandomNumber (1 to 9999) so this should be pretty close.
        /// </summary>
        /// <param name="_storeNumber">Store Number for use in eventually generated coupon code.</param>
        /// <param name="_orderNumber">Order Number for use in eventually generated coupon code.</param>
        /// <returns>String value indicating next unused coupon code.</returns>
        private static string GetUnusedCouponCodeForCampaign(string _storeNumber, string _orderNumber)
        {
            string couponPrefix = string.Concat(_storeNumber, _orderNumber);
            string ValueToReturn = "";
            int CriticalErrorCount = 0;

            for (int i = 0; i <= 9999; i++)
            {
                string CouponCodeToCheck = string.Concat(couponPrefix
                                                            , i.ToString().PadLeft(4, '0') //Append Zeros to front of i.ToString() (12 would become 0012)
                                                        );
                try
                {
                    if (HasCouponCodeNeverBeenUsed(CouponCodeToCheck))
                    {
                        //We found a good coupon code we can use. Break out of loop.
                        ValueToReturn = CouponCodeToCheck;
                        break;
                    }
                    else
                    {
                        throw new Exception("Order has already received a Shoe Buck.");
                        
                    }
                }
                catch (Exception e)
                {
                    //If we get a timeout or something we don't want to break the program. Instead let's wait till we get 3 big errors.
                    CriticalErrorCount++;
                    if (CriticalErrorCount >= 1)
                    {
                        //To many critical errors throw the exception.
                        throw e;
                    }
                }
            }

            if (ValueToReturn.Length > 30)
            {
                throw new Exception(string.Concat("To many characters for CouponCode return. Max 30. Value: ", ValueToReturn));
            }

            return ValueToReturn;
        }

        private static string GetCouponCodeForCampaignAndOrder(string _storeNumber, string _orderNumber)
        {
            string couponPrefix = string.Concat(_storeNumber, _orderNumber);
            string ValueToReturn = "";
            string CouponCodeToCheck = string.Concat(couponPrefix, "0000");

            if (HasCouponCodeNeverBeenUsed(CouponCodeToCheck))
            {
                //We found a good coupon code we can use. Break out of loop.
                ValueToReturn = CouponCodeToCheck;
            }

            return ValueToReturn;
        }

        /// <summary>
        /// Returns a boolean value indicating if CouponCodeToCheck already exists in Imagine's CouponCodesTable.
        /// </summary>
        /// <param name="CouponCodeToCheck">Coupon Code to check against Imagine DB.</param>
        /// <returns>boolean value indicate if Coupon does or doesn't exist.</returns>
        private static bool HasCouponCodeNeverBeenUsed(string CouponCodeToCheck)
        {
            ImagineDataAccess ImagineDAL = new ImagineDataAccess();
            ImagineDAL.RemoveAllSqlParameters();

            SqlParameter CouponCode = new SqlParameter("@CouponCodeToCheck", SqlDbType.VarChar, 30);

            CouponCode.Value = CouponCodeToCheck;

            ImagineDAL.AddSqlParameter(CouponCode);
            //ImagineDAL.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            ImagineDAL.QueryToRun = @"SELECT top 1 couponcode FROM CouponCodes WHERE CouponCode = @CouponCodeToCheck";
            DataSet results = ImagineDAL.returnQueryResultsDataSet();
            if (results.Tables.Count > 0)
            {
                if (results.Tables[0].Rows.Count == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static void WriteNewCouponToImagine(ShoeBuckCouponCode CampaignToUse)
        {
            ImagineDataAccess imaginedal = new ImagineDataAccess();
            //imaginedal.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            imaginedal.RemoveAllSqlParameters();

            //Adding coupon code as SQL parameter
            SqlParameter discount = new SqlParameter("@discount", System.Data.SqlDbType.VarChar, 10);
            SqlParameter coupon = new SqlParameter("@coupon", System.Data.SqlDbType.VarChar, 30);
            SqlParameter cType = new SqlParameter("@cType", System.Data.SqlDbType.VarChar, 30);
            SqlParameter start = new SqlParameter("@start", System.Data.SqlDbType.Date);
            SqlParameter stop = new SqlParameter("@stop", System.Data.SqlDbType.Date);
            SqlParameter description = new SqlParameter("@description", System.Data.SqlDbType.VarChar, 30);
            discount.Value = CampaignToUse.DiscountCode;
            coupon.Value = CampaignToUse.CouponCode;
            cType.Value = CampaignToUse.CouponType;
            start.Value = DateTime.Today.ToShortDateString();
            stop.Value = DateTime.Today.AddDays(CampaignToUse.NumberOfDaysValid).ToShortDateString();
            description.Value = CampaignToUse.Description;

            imaginedal.AddSqlParameter(discount);
            imaginedal.AddSqlParameter(coupon);
            imaginedal.AddSqlParameter(cType);
            imaginedal.AddSqlParameter(start);
            imaginedal.AddSqlParameter(stop);
            imaginedal.AddSqlParameter(description);

            imaginedal.QueryToRun = @"INSERT INTO CouponCodes (Discountcode, CouponCode, CouponType, StartDate, StopDate, CouponDesc, IsRedeemed)
                                      VALUES (@discount, @coupon, @cType, @start, @stop, @description, 'N')";

            string results = imaginedal.returnQueryResults();
            if (results.StartsWith("Reported Error:"))
            {
                throw new Exception(results);
            }
        }

        public static ShoeBuckCouponCode GetShoeBucksForOrder(decimal TotalSpent, string _storeNumber, string _orderNumber)
        {
            //See if we can find a Campaign for store 101
            ShoeBuckCouponCode CampaignToUse = FindEligibleCampaignForAmountStore(TotalSpent, "101");

            if (CampaignToUse.CampaignFound)
            {
                CampaignToUse.CouponCode = GetCouponCodeForCampaignAndOrder(_storeNumber, _orderNumber);
                if (CampaignToUse.CouponCode != "")
                {
                    CampaignToUse.CouponCodeGenerated = true;
                    WriteNewCouponToImagine(CampaignToUse);
                }
            }

            //Return what we came up with.
            return CampaignToUse;
        }

    }
}
