using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using MagentoStoreHoursUpdater;
using Company.Framework.DAL.MagentoDAL;
using Microsoft.Data.SqlClient;
using Nancy.Json;


namespace MagentoStoreUpdateByAPI
{
    public class Worker
    {
        public static void DoWork()
        {



            if (MagentoStoreUpdate.Default.CompareAndCreate)
            {
                CompareAndCreateStores();
            }
            if (MagentoStoreUpdate.Default.UpdateStoreInfo)
            {
                UpdateStoreInformation();
            }
            if (MagentoStoreUpdate.Default.UpdateStoreDesc)
            {
                UpdateStoreDescriptions();
            }
            if (MagentoStoreUpdate.Default.ImportStoreInfo)
            {
                ImportStoreInformationToFile();
            }
            
            
            //MagentoStoreHoursUpdater.Worker.DoWork();

        }


         /*This mehod will do the following:
          * 1. Obtain storecodes from Magento And Imagine DB
          * 2. Compare storecodes from Magento to Imagine DB
          * 3. Create new stores in Magento that do not currently exist
          */
        private static void CompareAndCreateStores()
        {
            string csvHeader = "code, name";  //address, city, region, postcode";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string filePath = string.Concat(desktopPath + "\\MageStoreAddresses.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);

            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fs);

            sw.Write(csvHeader);

            //Step 1
            //Obtain Storecodes from Magento and Imagine

            ImagineHelper imagineHelper = new ImagineHelper();
            MagentoHelperAPI mageHepler = new MagentoHelperAPI();
            string magentoStoreCodeString = mageHepler.GetAllStoreCodes();
            string imagineStoreCodeString = imagineHelper.GetListofStoresInGroupStores();

            JavaScriptSerializer existingStoresJSON = new JavaScriptSerializer();
            StoreInfo.ImagineStoreCode[] imagineStoreCodes = existingStoresJSON.Deserialize<StoreInfo.ImagineStoreCode[]>(imagineStoreCodeString);
            List<string> imagineStores = new List<string>();

            StoreInfo.MagentoStoreCode[] magentoStoreCodes = existingStoresJSON.Deserialize<StoreInfo.MagentoStoreCode[]>(magentoStoreCodeString);
            List<string> magentoStores = new List<string>();

            //if(imagineStoreCodes?.Count() > 0)
            //{
            //    foreach(var i in imagineStoreCodes)
            //    {
            //        imagineStores.Add(i.code.ToString());
            //        //Console.WriteLine(i.code.ToString());
            //        //Console.ReadLine();
            //    }

            //}
            

            //if (magentoStoreCodes?.Count() > 0)
            //{
                foreach (var m in magentoStoreCodes)
                {
                    magentoStores.Add(m.code.ToString());
                    //Console.WriteLine(m.code.ToString());
                    //Console.ReadLine();
                    
                    //Create New CSV File of Current Mageworx Store Addresses
                    MageStore store = existingStoresJSON.Deserialize<MageStore>(mageHepler.GetSingleStoreInformation(m.code.ToString()));
                    if(store?.code != null)
                {
                     sw.Write(string.Format("{0},/{3}/{4}/{1}{2}", store.code, store.name.Remove(store.name.Length-4,1).Replace(' ', '-'), Environment.NewLine, store.region, store.city));
                }
                   
                    //sw.Write(string.Format("{5}{0},\"{1}\",{2},{3},{4}", store.code, store.address, store.city, store.region, store.postcode, Environment.NewLine));
                }         
            //}

            sw.Close();
            fs.Close();

            //Step 2

            List<string> inImagineNotMagento = imagineStores.Except(magentoStores).ToList();

            //Step 3
            //Retrieve JSON String of stores that do not exist in Magento
            
            foreach(var store in inImagineNotMagento)
            {
                string jsonPayload = string.Concat("{\"location\": " + imagineHelper.GetStoresNotInMagentoInfo(store.ToString()) + "}");
                jsonPayload = jsonPayload.Replace("[", "");
                jsonPayload = jsonPayload.Replace("]", "");
                //Console.WriteLine(jsonPayload);
                //Console.ReadLine();

                mageHepler.CreateNewStoreInMagento(jsonPayload);

                imagineHelper.SetChangeDateForStores(store);
                
                //Create and Assign Source Locations
                //mageHepler.CreateSourceLocation(store);
                mageHepler.AssignSourceLocation(store);

                
            }

            

        }

        //Updates the current Mageworx Store (brick and mortar store) description in the SH_StoreDetails table in Imagine
        private static void UpdateStoreDescriptions()
        {
            ImagineHelper imagineHelper = new ImagineHelper();
            MagentoHelperAPI mageHepler = new MagentoHelperAPI();
            JavaScriptSerializer existingStoresJSON = new JavaScriptSerializer();

            string magentoStoreCodeString = mageHepler.GetAllStoreCodes();
            StoreInfo.MagentoStoreCode[] allMagentoStores = existingStoresJSON.Deserialize<StoreInfo.MagentoStoreCode[]>(magentoStoreCodeString);
            List<string> completeMagentoStoreList = new List<string>();

            if (allMagentoStores?.Count() > 0)
            {
                foreach (var a in allMagentoStores)
                {
                    completeMagentoStoreList.Add(a.code.ToString());
                    //Console.WriteLine(a.code.ToString());
                    //Console.ReadLine();
                }
            }

            foreach (var store in completeMagentoStoreList)
            {
                string aMageStoreString = mageHepler.GetSingleStoreInformation(store);
                MageStore aMageStore = existingStoresJSON.Deserialize<MageStore>(aMageStoreString);

                imagineHelper.UpdateWebDescriptions(aMageStore.code, aMageStore.description);

                //Console.WriteLine(store);
                //Console.ReadLine();
            }


        }



        /// <summary>
        /// Goes through all Magento Stores pulls down the Store's Description, Reformats it, and then push the update back to the website.
        /// The idea behind this is that the HTML blocks that make up the Store's description aren't getting saved correctly on the MageWorx stores page.
        /// </summary>
        private static void UpdateStoreInformation()
        {
            string SeasonalMessage = "Looking forward to seeing you in the new year!";

            if(SeasonalMessage.Length > 0)
            {
                SeasonalMessage = string.Format("<P>{0}</P>", SeasonalMessage);
            }

            MagentoHelperAPI MagentoDAL = new MagentoHelperAPI();
            JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

            //Retrieving all store codes from Magento
            StoreInfo.MagentoStoreCode[] allMagentoStores = JsonSerializer.Deserialize<StoreInfo.MagentoStoreCode[]>(MagentoDAL.GetAllStoreCodes());

            foreach (StoreInfo.MagentoStoreCode store in allMagentoStores)
            {
                if (store.code != "132" && store.code == "973")
                {
                    //Get's the single store information from Magento
                    MageStore StoreMagentoInfo = JsonSerializer.Deserialize<MageStore>(MagentoDAL.GetSingleStoreInformation(store.code));

                    StoreUpdateHelperJson NewDescription = new StoreUpdateHelperJson(StoreMagentoInfo.code, StoreMagentoInfo.description, SeasonalMessage);

                    string jsonPayload = JsonSerializer.Serialize(NewDescription);
                    
                    string newstring = MagentoDAL.UpdateStoreInformation(jsonPayload, store.code);
                    Console.WriteLine(newstring);
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

            //Imports store information and creates a .csv file for stores that are missing descriptions
            private static void ImportStoreInformationToFile()
        {
            
            MagentoHelperAPI mageHepler = new MagentoHelperAPI();
            string magentoStoreCodeString = mageHepler.GetAllStoreCodes();
            JavaScriptSerializer existingStoresJSON = new JavaScriptSerializer();
            StoreInfo.MagentoStoreCode[] magentoStoreCodes = existingStoresJSON.Deserialize<StoreInfo.MagentoStoreCode[]>(magentoStoreCodeString);
            List<MagentoStoreObj> magentoStores = new List<MagentoStoreObj>();
            Parallel.ForEach(magentoStoreCodes, store =>
            {
                magentoStores.Add(MagentoDALHelper.GetSingleStoreInformation(store.code));
            }
            );



            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string filePath = string.Concat(desktopPath + "\\Stores_Missing_Descriptions.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);

            }

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fs);

            string csvHeader = string.Format("StoreNumber, City, State");
            sw.WriteLine(csvHeader);

            foreach(MagentoStoreObj storeDetail in magentoStores)
            {
                if(storeDetail.description is null)
                {
                    sw.WriteLine(string.Format("{0},{1},{2}", storeDetail.code, storeDetail.city, storeDetail.region, storeDetail.website_url));
                }
            }

            sw.Close();
            fs.Close();
            
        }
    }


    public class Data
    {
        public string code { get; set; }
        public string description { get; set; }
        public string name { get; set; }

        public Data(string StoreNumber, string StoreDescription)
        {
            code = StoreNumber;
            SetDescription(StoreDescription);
            SetNameFromImagineDB(StoreNumber);
        }

        public Data(string StoreNumber, string StoreDescription, string SeasonalMessage)
        {
            code = StoreNumber;
            SetDescription(StoreDescription, SeasonalMessage);
            SetNameFromImagineDB(StoreNumber);
        }

        private void SetNameFromImagineDB(string StoreNumber)
        {
            name = "";

            Company.Framework.DAL.ImagineDataAccess imagineDAL = new Company.Framework.DAL.ImagineDataAccess();
            imagineDAL.QueryToRun = "select City, State from Store where StoreNumber = @StoreNumber";

            imagineDAL.RemoveAllSqlParameters();
            
            SqlParameter storeString = new SqlParameter("@StoreNumber", System.Data.SqlDbType.VarChar, 5)
            {
                Value = StoreNumber
            };
            storeString.Value = StoreNumber;
            imagineDAL.AddSqlParameter(storeString);

            System.Data.DataSet StoreInfo = imagineDAL.returnQueryResultsDataSet();
            if(StoreInfo.Tables.Count > 0)
            {
                if(StoreInfo.Tables[0].Rows.Count > 0)
                {
                    name = string.Format("Shoe Sensation in {0}, {1}"
                                            , StoreInfo.Tables[0].Rows[0]["City"].ToString()
                                            , StoreInfo.Tables[0].Rows[0]["State"].ToString()
                                        );
                }
            }

            if(name == "")
            {
                throw new Exception(string.Concat("Unable to pull information for storecode ",StoreNumber, " in Imagine."));
            }
        }

        private void SetDescription(string NewDescription)
        {
            SetDescription(NewDescription, "");
        }

        private void SetDescription(string NewDescription, string SeasonalMessage)
        {
            //For whatever reason < and > get's encoded as their HTML escape characters on inital description create, this line of code fixes that. 
            string TransformedDescription = NewDescription.Replace("&lt;", "<").Replace("&gt;", " >");

            TransformedDescription = AddSeasonalMessageToDescriptionString(TransformedDescription, SeasonalMessage);

            //Encode string as UTF-8 to normalize other characters.
            byte[] bytes = Encoding.Default.GetBytes(TransformedDescription);
            description = Encoding.UTF8.GetString(bytes);
        }

        private string AddSeasonalMessageToDescriptionString(string ValueToEdit, string SeasonalMessage)
        {
            //Look for <Div Class="SeasonalMSG">
            int DivStartIndex = ValueToEdit.IndexOf("<Div Class=\"SeasonalMSG\">");

            //If we didn't find the div add it to the end of the description.
            if(DivStartIndex == -1)
            {
                DivStartIndex = ValueToEdit.Length;
                ValueToEdit = string.Concat(ValueToEdit, "<Div Class=\"SeasonalMSG\"></Div>");
            }

            //Account for Characters in Div declaration in Start Index.
            DivStartIndex = DivStartIndex + 25; 

            //Determine index of closing div.
            int DivEndIndex = ValueToEdit.IndexOf("</Div>", DivStartIndex);


            //Remove Text in Seasonal Message Div
            if (DivEndIndex != DivStartIndex)
            {
                ValueToEdit = ValueToEdit.Remove(DivStartIndex, DivEndIndex - DivStartIndex);
            }

            //Add in New Text
            ValueToEdit = ValueToEdit.Insert(DivStartIndex, SeasonalMessage);

            return ValueToEdit;
        }

    }

    public class StoreUpdateHelperJson
    {
        public Data data { get; set; }

        public StoreUpdateHelperJson(string StoreNumber, string StoreDescription)
        {
            data = new Data(StoreNumber, StoreDescription);
        }

        public StoreUpdateHelperJson(string StoreNumber, string StoreDescription, string SeasonalDescription)
        {
            data = new Data(StoreNumber, StoreDescription, SeasonalDescription);
        }
    }
}
