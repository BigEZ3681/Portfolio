using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ShoeSensation.Framework.DAL;

namespace MagentoStoreUpdateByAPI
{
    public class ImagineHelper
    {
        public string GetUpdatedStoreInformation(string storeNumber, string _description)
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            imagineDAL.RemoveAllSqlParameters();
            SqlParameter storeString = new SqlParameter("@storeString", System.Data.SqlDbType.VarChar, 5);
            SqlParameter description = new SqlParameter("@description", System.Data.SqlDbType.VarChar, 2000);
            storeString.Value = storeNumber;
            description.Value = _description;
            imagineDAL.AddSqlParameter(storeString);
            imagineDAL.AddSqlParameter(description);


            imagineDAL.QueryToRun = @"SELECT s.StoreNumber as [code]
                                        , @description AS [description]                                        
                                        ,  CONCAT('Shoe Sensation in ', LEFT(City,1)+LOWER(SUBSTRING(City,2,LEN(City))), ', ', State) as [name]
                                      FROM Store s
                                      LEFT JOIN SH_StoreDetails d ON s.StoreNumber = d.StoreNumber
                                      WHERE s.StoreNumber = @storeString";

            return imagineDAL.returnJSONString();
        }

        //Sets the ChangeDate field for the new Brick and Mortar store's permanent hours.
        public string SetChangeDateForStores(string storeNumber)
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            imagineDAL.RemoveAllSqlParameters();
            SqlParameter aStoreNumber = new SqlParameter("@AStoreNumber", System.Data.SqlDbType.VarChar, 5);
            aStoreNumber.Value = storeNumber;
            imagineDAL.AddSqlParameter(aStoreNumber);
            imagineDAL.QueryToRun = @"UPDATE SH_StoreOperatingHours
                                        SET ChangeDate = GETDATE()
                                        WHERE StoreNumber = @AStoreNumber AND Type = 'P'";

            return imagineDAL.runNonQueryResults();
        }

        //Retrieves a list of brick and mortar stores in Imagine
        public string GetListofStoresInGroupStores()
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            imagineDAL.QueryToRun = @"SELECT StoreNumber AS [Code] FROM GroupStores WHERE GroupName = 'ONLINE_PU'";


            return imagineDAL.returnJSONString();

        }

        //Retrieves new brick and mortar location information to pass to Magento
        public string GetStoresNotInMagentoInfo(string currentImagineStore)
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            imagineDAL.RemoveAllSqlParameters();
            SqlParameter storeString = new SqlParameter("@storeString", System.Data.SqlDbType.VarChar, 5);
            storeString.Value = currentImagineStore;
            imagineDAL.AddSqlParameter(storeString);
            imagineDAL.QueryToRun = @"SELECT s.StoreNumber AS [code]
                , CONCAT(LEFT(City,1)+LOWER(SUBSTRING(City,2,LEN(City))), ', ', State, ' Shoe Sensation ') as [name]
	            , CONCAT('Shoe Sensation in '
				, UPPER(LEFT(City,1))+LOWER(SUBSTRING(city,2,LEN(City)))
				, ', '
				, State
				, '.  Shop our boots, sandals, dress, and athletic shoes for your entire family at your local Shoe Sensation.  Get a first look at brand new shoes coming in at your local Shoe Sensation store located at '
				--, case when IsNull(d.USPS_VerifiedStreetAddress,'') = '' then Concat(s.Address1,' ',s.Address2) else d.USPS_VerifiedStreetAddress end
				,	stuff((
					   select ' '+upper(left(T3.V, 1))+lower(stuff(T3.V, 1, 1, ''))
					   from (select cast(replace((select case when IsNull(d.USPS_VerifiedStreetAddress,'') = '' then Concat(s.Address1,' ',s.Address2) else d.USPS_VerifiedStreetAddress end as '*' for xml path('')), ' ', '<X/>') as xml).query('.')) as T1(X)
						 cross apply T1.X.nodes('text()') as T2(X)
						 cross apply (select T2.X.value('.', 'varchar(30)')) as T3(V)
					   for xml path(''), type
					   ).value('text()[1]', 'varchar(30)'), 1, 1, '')
				, ' '
				,  UPPER(LEFT(City,1))+LOWER(SUBSTRING(city,2,LEN(City)))
				, ', '
				, State
				, '.  Our Sales employees will be happy to assist you in finding the perfect pair of shoes, whether it’s snow boots for the winter, or those cute sandals you have been eyeing for the spring. Be sure to check out our Best Deal specials throughout the store to get your low-priced footwear!  Shop our women’s, men’s, and kid’s selections, including brands like Converse, Nike, Adidas, Vans, Under Armour, Hey Dudes, Skechers, Converse, Puma, and Fila to find the best running and casual sneakers to suit your needs.  Your nearby Shoe Sensation in '
				,  UPPER(LEFT(City,1))+LOWER(SUBSTRING(city,2,LEN(City)))
				, ', ', State
				, ' is always getting new styles in, so be sure to check us out frequently to see our new deals and specials on shoes!') AS [description]
	            ,	stuff((
					   select ' '+upper(left(T3.V, 1))+lower(stuff(T3.V, 1, 1, ''))
					   from (select cast(replace((select case when IsNull(d.USPS_VerifiedStreetAddress,'') = '' then Concat(s.Address1,' ',s.Address2) else d.USPS_VerifiedStreetAddress end as '*' for xml path('')), ' ', '<X/>') as xml).query('.')) as T1(X)
						 cross apply T1.X.nodes('text()') as T2(X)
						 cross apply (select T2.X.value('.', 'varchar(30)')) as T3(V)
					   for xml path(''), type
					   ).value('text()[1]', 'varchar(30)'), 1, 1, '')  as [address]
                                            , '0' AS [store_ids]
	                                        , UPPER(LEFT(City,1))+LOWER(SUBSTRING(City,2,LEN(City))) AS [city]
	                                        , CASE
                                                    WHEN State =  'AL' THEN 'Alabama'
                                                    WHEN State =  'AK' THEN 'Alaska'
                                                    WHEN State =  'AZ' THEN 'Arizona'
                                                    WHEN State =  'AR' THEN 'Arkansas'
                                                    WHEN State =  'CA' THEN 'California'
                                                    WHEN State =  'CO' THEN 'Colorado'
                                                    WHEN State =  'CT' THEN 'Connecticut'
                                                    WHEN State =  'DE' THEN 'Delaware'
                                                    WHEN State =  'DC' THEN 'District of Columbia'
                                                    WHEN State =  'FL' THEN 'Florida'
                                                    WHEN State =  'GA' THEN 'Georgia'
                                                    WHEN State =  'HI' THEN 'Hawaii'
                                                    WHEN State =  'ID' THEN 'Idaho'
                                                    WHEN State =  'IL' THEN 'Illinois'
                                                    WHEN State =  'IN' THEN 'Indiana'
                                                    WHEN State =  'IA' THEN 'Iowa'
                                                    WHEN State =  'KS' THEN 'Kansas'
                                                    WHEN State =  'KY' THEN 'Kentucky'
                                                    WHEN State =  'LA' THEN 'Louisiana'
                                                    WHEN State =  'ME' THEN 'Maine'
                                                    WHEN State =  'MD' THEN 'Maryland'
                                                    WHEN State =  'MA' THEN 'Massachusetts'
                                                    WHEN State =  'MI' THEN 'Michigan'
                                                    WHEN State =  'MN' THEN 'Minnesota'
                                                    WHEN State =  'MS' THEN 'Mississippi'
                                                    WHEN State =  'MO' THEN 'Missouri'
                                                    WHEN State =  'MT' THEN 'Montana'
                                                    WHEN State =  'NE' THEN 'Nebraska'
                                                    WHEN State =  'NV' THEN 'Nevada'
                                                    WHEN State =  'NH' THEN 'New Hampshire'
                                                    WHEN State =  'NJ' THEN 'New Jersey'
                                                    WHEN State =  'NM' THEN 'New Mexico'
                                                    WHEN State =  'NY' THEN 'New York'
                                                    WHEN State =  'NC' THEN 'North Carolina'
                                                    WHEN State =  'ND' THEN 'North Dakota'
                                                    WHEN State =  'OH' THEN 'Ohio'
                                                    WHEN State =  'OK' THEN 'Oklahoma'
                                                    WHEN State =  'OR' THEN 'Oregon'
                                                    WHEN State =  'PA' THEN 'Pennsylvania'
                                                    WHEN State =  'PR' THEN 'Puerto Rico'
                                                    WHEN State =  'RI' THEN 'Rhode Island'
                                                    WHEN State =  'SC' THEN 'South Carolina'
                                                    WHEN State =  'SD' THEN 'South Dakota'
                                                    WHEN State =  'TN' THEN 'Tennessee'
                                                    WHEN State =  'TX' THEN 'Texas'
                                                    WHEN State =  'UT' THEN 'Utah'
                                                    WHEN State =  'VT' THEN 'Vermont'
                                                    WHEN State =  'VA' THEN 'Virginia'
                                                    WHEN State =  'WA' THEN 'Washington'
                                                    WHEN State =  'WV' THEN 'West Virginia'
                                                    WHEN State =  'WI' THEN 'Wisconsin'
                                                    WHEN State =  'WY' THEN 'Wyoming'
                                               END AS [region]
	                                        , ZipCode AS [postcode]
                                            , 'US' AS [country_id]
                                            , CONCAT('+1 ', REPLACE(Phone, '-', ' ')) as [phone_number]
	                                        , Store_Latitude AS [latitude]
	                                        , Store_Longitude AS [longitude]
                                            , 'false' as [is_active]
                                    FROM Store s
                                    LEFT JOIN SH_StoreDetails d ON s.StoreNumber = d.StoreNumber
                                    WHERE s.StoreNumber = @storeString ";
          
            return imagineDAL.returnJSONString();

        }

        //Updates WebDescriptions In SH_StoreDetails Table
        public void UpdateWebDescriptions(string currentImagineStore, string description)
        {
            ImagineDataAccess imagineDAL = new ImagineDataAccess();
            imagineDAL.RemoveAllSqlParameters();
            SqlParameter storeString = new SqlParameter("@storeString", System.Data.SqlDbType.VarChar, 5);
            SqlParameter descriptionString = new SqlParameter("@WebsiteDescription", System.Data.SqlDbType.VarChar, 2000);
            storeString.Value = currentImagineStore;
            descriptionString.Value = description;
            imagineDAL.AddSqlParameter(storeString);
            imagineDAL.AddSqlParameter(descriptionString);
            imagineDAL.QueryToRun = @"UPDATE SH_StoreDetails
                                      SET WebsiteDescription = @WebsiteDescription
                                      WHERE StoreNumber = @storeString";

            imagineDAL.runNonQueryResults();
        }
    }
}
