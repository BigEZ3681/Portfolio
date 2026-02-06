using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagentoStoreUpdateByAPI
{
     class MageStore
    {
        public string code { get; set; }
        public string assign_type { get; set; }
        public bool apply_by_cron { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; }
        public int order { get; set; }
        public string image_path { get; set; }
        public string country { get; set; }
        public string country_id { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string postcode { get; set; }
        public object email { get; set; }
        public string phone_number { get; set; }
        public object website_url { get; set; }
        public object skype { get; set; }
        public object whatsapp { get; set; }
        public object instagram { get; set; }
        public object facebook { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public List<string> store_ids { get; set; }
        public List<object> product_skus { get; set; }
        public string working_hours_type { get; set; }
        public List<string> working_hours { get; set; }
        public string location_page_path { get; set; }
        public bool open_now { get; set; }
        public string working_hours_info { get; set; }
        public string source_code { get; set; }
        public List<string> prepared_data_for_customer { get; set; }



    }

    class StoreInfo
    {

        #region Old Code

 
        public class MagentoStoreCode
        {
            public string code;
        }
        

       public class ImagineStoreCode
        {
            public string code;
        }
        


    }
}
        
