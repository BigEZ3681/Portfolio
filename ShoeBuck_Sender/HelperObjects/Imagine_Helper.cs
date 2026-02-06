using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using ShoeSensation.Framework.DAL;
using ShoeSensation.Framework.DAL.ListrakDAL;

namespace ShoeBuck_Sender.HelperObjects
{
    public class ImagineOrderObject
    {
        private string storeNumber;
        private string orderNumber;
        private string email;
        private string firstName;
        private string lastName;
        private string pODDate;
        private string lastShipment;
        private string shoeBuckCheckedFlag;
        private string shipmentCount;
        private string shippingAmount;
        private string extendedRetail;
        private string extendedItemTotal;
        private string orderTotalPayments;
        private ShoeBuckCouponCode generatedCampaignCode;

        public ImagineOrderObject(string _StoreNumber, string _OrderNumber, string _Email, string _FirstName, string _LastName, string _PODDate, string _LastShipment, string _ShoeBuckCheckedFlag, string _ShipmentCount, string _ShippingAmount, string _ExtendedRetail, string _ExtendedItemTotal, string _OrderTotalPayments)
        {
            storeNumber = _StoreNumber;
            orderNumber = _OrderNumber;
            email = _Email;
            firstName = _FirstName;
            lastName = _LastName;
            pODDate = _PODDate;
            lastShipment = _LastShipment;
            shoeBuckCheckedFlag = _ShoeBuckCheckedFlag;
            shipmentCount = _ShipmentCount;
            shippingAmount = _ShippingAmount;
            extendedRetail = _ExtendedRetail;
            extendedItemTotal = _ExtendedItemTotal;
            orderTotalPayments = _OrderTotalPayments;
            generatedCampaignCode = null;
        }

        public string StoreNumber { get => storeNumber; set => storeNumber = value; }
        public string OrderNumber { get => orderNumber; set => orderNumber = value; }
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
            } 
        }
        public string FirstName { get => firstName; set => firstName = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string PODDate { get => pODDate; set => pODDate = value; }
        public string LastShipment { get => lastShipment; set => lastShipment = value; }
        public string ShoeBuckCheckedFlag { get => shoeBuckCheckedFlag; set => shoeBuckCheckedFlag = value; }
        public int ShipmentCount { get => IntHelper(shipmentCount); }
        public decimal ShippingAmount { get => MoneyHelper(shippingAmount); }
        public decimal ExtendedRetail { get => MoneyHelper(extendedRetail); }
        public decimal ExtendedItemTotal { get => MoneyHelper(extendedItemTotal); }
        public decimal OrderTotalPayments { get => MoneyHelper(orderTotalPayments); }
        public ShoeBuckCouponCode GeneratedCampaignCode { get => generatedCampaignCode; }

        private int IntHelper(string ValToParse)
        {
            int returnValue = 0;
            if(int.TryParse(ValToParse, out returnValue))
            {
                return returnValue;
            }
            return 0;
        }

        private decimal MoneyHelper(string ValToParse)
        {
            decimal returnValue = 0;
            if (decimal.TryParse(ValToParse, out returnValue))
            {
                return returnValue;
            }
            return 0;
        }

        public void MarkOrderAsHavingBeenCheckedForShoeBuckEligibility()
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            //imagineDAL.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            imagineDAL.RemoveAllSqlParameters();
            SqlParameter orderNumberParam = new SqlParameter("@OrderNumber", System.Data.SqlDbType.VarChar, 15);
            SqlParameter campaignfound = new SqlParameter("@CampaignFound", System.Data.SqlDbType.Bit);

            orderNumberParam.Value = orderNumber;
            campaignfound.Value = 1;
            if (GeneratedCampaignCode == null)
            {
                campaignfound.Value = 0;
            }

            imagineDAL.AddSqlParameter(orderNumberParam);
            imagineDAL.AddSqlParameter(campaignfound);

            //Update all records with that order number related to shipments in the last year.
            imagineDAL.QueryToRun = @"UPDATE ShipmentTracking
                                        SET ShoeBuckChecked = case when ShoeBuckChecked is null then @CampaignFound else ShoeBuckChecked end
                                        WHERE ReferenceNumber = @OrderNumber
                                            AND ShipDate >= DateAdd(Year, -1, Getdate())";

            string results = imagineDAL.runNonQueryResults();

            if (results.StartsWith("Reported Error"))
            {
                throw new Exception(results);
            }
        }

        public void WriteToOelog(string Message)
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            //imagineDAL.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            imagineDAL.RemoveAllSqlParameters();
            SqlParameter storeNumberParam = new SqlParameter("@StoreNumber", System.Data.SqlDbType.VarChar, 10);
            SqlParameter orderNumberParam = new SqlParameter("@OrderNumber", System.Data.SqlDbType.VarChar, 10);
            SqlParameter functionNameParam = new SqlParameter("@FunctionName", System.Data.SqlDbType.VarChar, 50);
            SqlParameter actionParam = new SqlParameter("@Action", System.Data.SqlDbType.VarChar, 800);

            storeNumberParam.Value = StoreNumber;
            orderNumberParam.Value = OrderNumber;
            functionNameParam.Value = "SHOEBUCK_SENDER";
            actionParam.Value = Message;

            imagineDAL.AddSqlParameter(storeNumberParam);
            imagineDAL.AddSqlParameter(orderNumberParam);
            imagineDAL.AddSqlParameter(functionNameParam);
            imagineDAL.AddSqlParameter(actionParam);

            imagineDAL.QueryToRun = @"insert into OELog (StoreNumber, OrderNumber, ProcessingStore, FunctionName, UserName, Action, TimeStamp)
                                      values (@StoreNumber, @OrderNumber, '', @FunctionName, '', @Action, getdate())";

            string result = imagineDAL.runNonQueryResults();
            if (result.StartsWith("Reported Error"))
            {
                throw new Exception(result);
            }

        }

        public void DetermineCampaignCodeToSend()
        {
            generatedCampaignCode = ShoeBucksHelper.GetShoeBucksForOrder(ExtendedRetail, storeNumber, OrderNumber);
            if(generatedCampaignCode.CouponCodeGenerated)
            {
                WriteToOelog(string.Concat("Campaign Code Generated for Customer: ", generatedCampaignCode.CouponCode));
                return;
            }
            WriteToOelog("We found a shipment for order, but determined no Campaign Code was needed");
        }

        public void SendShoebucksEmailThroughLisTrack()
        {
            ListTrakAPIPoints LisTrackAPIHelper = new ListTrakAPIPoints();

            SingleItemTransactionalData Item = ListrakTransactionaEmailObj.CreateTransactionalEmailObjectSimple(Email);
            Item.UpdateElement(Multi_Items_NonItem_FieldValues.CustomerFirstName, JustifyNameCase(FirstName));
            Item.UpdateElement(Multi_Items_NonItem_FieldValues.CustomerLastName, JustifyNameCase(LastName));
            Item.UpdateElement(Multi_Items_NonItem_FieldValues.OrderNumber, OrderNumber);
            Item.UpdateElement(Multi_Items_NonItem_FieldValues.ShoeBuck, generatedCampaignCode.ToListrackTransationalDiv(OrderNumber));
            Item.UpdateElement(Multi_Items_NonItem_FieldValues.ShoeBuckCode, generatedCampaignCode.CouponCode.ToString());
            Item.UpdateElement(Multi_Items_NonItem_FieldValues.ShoeBuckExpiration, DateTime.Today.AddDays(generatedCampaignCode.NumberOfDaysValid).ToString("MM/dd/yyyy"));

            string SerializedItem = JsonConvert.SerializeObject(Item);
            //If a coupon is generated, send it to the customer using Listrak
            LisTrackAPIHelper.SendShoeBuckForEligibleOrders(SerializedItem);
            WriteToOelog("Shoe Bucks Email sent through LisTrack");
        }

        private string JustifyNameCase(string _name)
        {
            string name = _name.ToLower();
            
            //Substring will fail if we dont have any characters.
            if (!string.IsNullOrWhiteSpace(_name))
            {
                string nameFirstInitial = name.Substring(0).ToUpper();
                name = _name.Replace(name.Substring(0), nameFirstInitial);
            }
            return name;
        }

        public override string ToString()
        {
            return string.Concat(StoreNumber, '-', OrderNumber,'-',Email);
        }
    }


    class Imagine_Helper
    {
        //Querying Imagine for orders that have not been checked for Shoe Buck Eligibility.
        public static List<ImagineOrderObject> GetPickupOrders()
        {
            List<ImagineOrderObject> orderListToReturn = new List<ImagineOrderObject>();

            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            //imagineDAL.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            imagineDAL.RemoveAllSqlParameters();

            imagineDAL.QueryToRun = @"
                                        select OrderBase.StoreNumber
	                                        ,	OrderBase.OrderNumber
	                                        ,	OrderBase.Email
	                                        ,	OrderBase.FirstName
	                                        ,	OrderBase.LastName
	                                        ,	OrderBase.PODDate
	                                        ,	OrderBase.LastShipment
	                                        ,	OrderBase.ShoeBuckCheckedFlag
	                                        ,	OrderBase.ShipmentCount
	                                        ,	Max(OrderBase.ShippingAmount) as ShippingAmount
	                                        ,	Sum(d.Qty * d.Price) as ExtendedRetail
	                                        ,	Sum(d.Qty * (d.Price + d.TaxAmount1 + d.TaxAmount2 + d.TaxAmount3 + d.TaxAmount4 + d.TaxAmount5)) as ExtendedItemTotal
	                                        ,	Max(OrderBase.ShippingAmount) + Sum(d.Qty * (d.Price + d.TaxAmount1 + d.TaxAmount2 + d.TaxAmount3 + d.TaxAmount4 + d.TaxAmount5)) as OrderTotalPayments
	                                        from
	                                        (
		                                        select h.StoreNumber
			                                        ,	h.OrderNumber
			                                        ,	h.Email
			                                        ,	h.FirstName
			                                        ,	h.LastName
			                                        ,	Max(PODDate)	as PODDate
			                                        ,	Max(ShipDate)	as LastShipment
			                                        ,	Max(case when ShoeBuckChecked is null then 'N' else 'Y' end) as ShoeBuckCheckedFlag
			                                        ,	Count(Distinct TrackingNumber) as ShipmentCount
			                                        ,	ShippingAmount
			                                        from ShipmentTracking as s
			                                        inner join OEHdr as h on s.ReferenceNumber = h.OrderNumber and h.PickupOrder = 'Y'		--Only grab Pickup Orders
			                                        where s.ShipDate >= DateAdd(Month, -2, getdate())										--Grab Shipments for the last 2 Months for Summary Details.
			                                        Group by h.StoreNumber
			                                        ,	h.OrderNumber
			                                        ,	h.Email
			                                        ,	h.FirstName
			                                        ,	h.LastName
			                                        ,	ShippingAmount
			                                        Having Max(ShipDate) >= DateAdd(Day, -2, getdate())										--Only touch items that were shipped in the last 2 days.
				                                        and Max(case when ShoeBuckChecked is null then 'N' else 'Y' end) = 'N'				--Where we haven't done a ShoeBucks check.
	                                        ) as OrderBase
	                                        Left Join Oedtl as d on OrderBase.StoreNumber = d.StoreNumber and OrderBase.OrderNumber = d.OrderNumber
	                                        Group by OrderBase.StoreNumber
	                                        ,	OrderBase.OrderNumber
	                                        ,	OrderBase.Email
	                                        ,	OrderBase.FirstName
	                                        ,	OrderBase.LastName
	                                        ,	OrderBase.PODDate
	                                        ,	OrderBase.LastShipment
	                                        ,	OrderBase.ShoeBuckCheckedFlag
	                                        ,	OrderBase.ShipmentCount
                                        ";
 
            DataSet ReturnedItems = imagineDAL.returnQueryResultsDataSet();

            //Write each returned row to custom item type.
            foreach (DataRow row in ReturnedItems.Tables[0].Rows)
            {
                orderListToReturn.Add(new ImagineOrderObject(row["StoreNumber"].ToString()
                                                                , row["OrderNumber"].ToString()
                                                                , row["Email"].ToString()
                                                                , row["FirstName"].ToString()
                                                                , row["LastName"].ToString()
                                                                , row["PODDate"].ToString()
                                                                , row["LastShipment"].ToString()
                                                                , row["ShoeBuckCheckedFlag"].ToString()
                                                                , row["ShipmentCount"].ToString()
                                                                , row["ShippingAmount"].ToString()
                                                                , row["ExtendedRetail"].ToString()
                                                                , row["ExtendedItemTotal"].ToString()
                                                                , row["OrderTotalPayments"].ToString()));
            }
            return orderListToReturn;
        }

        public static void RunShoeBucksCleanupQuery()
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            //imagineDAL.ConnectionString = "Data Source=192.168.127.26;Initial Catalog=IMAGINE_TEST;User Id=manager;Password=smartretail;";
            imagineDAL.RemoveAllSqlParameters();

            imagineDAL.QueryToRun = @"Update st
	                                    set ShoeBuckChecked = tmp1.ShoeBuckChecked
	                                    from 
	                                    (
		                                    /* Grab all Shipments sharing a reference number with an order that was shipped in the last week.*/
		                                    select ReferenceNumber
			                                    ,	CAST(MAX(CAST(ShoeBuckChecked as INT)) AS BIT) as ShoeBuckChecked
			                                    from ShipmentTracking
			                                    where ReferenceNumber in (select OrderNumber from Oedtl where ShipDate >= DateADD(Week, -2, getdate()))
			                                    Group by ReferenceNumber
			                                    Having CAST(MAX(CAST(ShoeBuckChecked as INT)) AS BIT) is not Null
	                                    ) as tmp1
	                                    inner join ShipmentTracking as st on tmp1.ReferenceNumber = st.ReferenceNumber and st.ShoeBuckChecked is null and ShipDate >= DateAdd(Month, -3, getdate())";

            string result = imagineDAL.runNonQueryResults();

            if(result.StartsWith("Reported Error"))
            {
                throw new Exception(result);
            }
        }
    }

}
