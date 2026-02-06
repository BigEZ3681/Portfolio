using ShoeBuck_Sender.HelperObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeBuck_Sender
{
    class Worker
    {
        public static int DoWork()
        {
            int ReturnCode = 0;

            try
            {
                //Get a list of Pickup Orders to check
                List<ImagineOrderObject> ecomPickupOrders = Imagine_Helper.GetPickupOrders();


                foreach (ImagineOrderObject order in ecomPickupOrders)
                {
                    //For each order, see if we need to send a coupon
                    order.DetermineCampaignCodeToSend();
                    if (order.GeneratedCampaignCode.CouponCodeGenerated)
                    {
                        //If so send the coupon
                        order.SendShoebucksEmailThroughLisTrack();
                    }
                    //Regardless update the order.
                    order.MarkOrderAsHavingBeenCheckedForShoeBuckEligibility();
                }

            }
            catch (Exception e)
            {
                //On error send email.
                ShoeSensation.Framework.Email_Helper.SendEmail("Monitor@shoesensation.com"
                                                                , ShoeBuck_Sender.Default.EmailError
                                                                , "Error with Shoe Bucks Sender"
                                                                , string.Format("Error sending Shoe Bucks Notification.{0}{1}", Environment.NewLine, e.ToString())
                                                                , ""
                                                                , "192.168.127.250"
                                                                , new List<string>());
            }

            try
            {
                //Backfill "Shoebuck checked" on all shipments sharing OrderNumbers that were shipped in the last couple months.
                Imagine_Helper.RunShoeBucksCleanupQuery();
            }
            catch (Exception e)
            {
                //On error send email.
                ShoeSensation.Framework.Email_Helper.SendEmail("Monitor@shoesensation.com"
                                                                , ShoeBuck_Sender.Default.EmailError
                                                                , "Error with Shoe Bucks Sender"
                                                                , string.Format("Error running ShoeBucks cleanup query.{0}{1}", Environment.NewLine, e.ToString())
                                                                , ""
                                                                , "192.168.127.250"
                                                                , new List<string>());
            }
            return ReturnCode;
        }
    }
}
