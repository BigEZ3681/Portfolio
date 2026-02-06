using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace MagentoStoreUpdateByAPI
{
    public class MagentoHelperAPI
    {
        // Get Single Store Information From Magento
        public string GetSingleStoreMagentoInformation(string storeNumber)
        {
            var client = new RestClient(string.Format("https://staging.shoesensation.com/rest/V1/mwlocations/{0}", storeNumber));
            var request = new RestRequest() { Method = Method.Get };
            // Set per-request timeout if required (milliseconds). -1 means infinite in older code; keep default or set a value.
            request.Timeout = TimeSpan.FromSeconds(10);
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Cookie", "PHPSESSID=a42a61f2c23973bd2ffe394cd12455b3; PHPSESSID=a42a61f2c23973bd2ffe394cd12455b3");
            var body = string.Empty;
            // For empty body, we can skip adding one; keep original intent:
            request.AddStringBody(body, DataFormat.None);
            RestResponse response = client.Execute(request);

            return response.Content;
        }

        // Updates store information within Magento
        public string UpdateStoreInformation(string jsonPayload, string storeNumber)
        {
            var client = new RestClient(string.Format("https://staging.shoesensation.com/rest/V1/mwlocations/update/{0}", storeNumber));
            var request = new RestRequest() { Method = Method.Post };
            // Request-level TLS is controlled globally; keep original call for compatibility if needed
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=bf68c7713e5a49f3a3b64a2af24efd5f; mage-messages=%5B%7B%22type%22%3A%22error%22%2C%22text%22%3A%22Invalid%20Form%20Key.%20Please%20refresh%20the%20page.%22%7D%5D; private_content_version=d2f9cbd9190900d783af6e84acf8b471");
            // Use AddStringBody to send raw JSON
            request.AddStringBody(jsonPayload, DataFormat.Json);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            ThrowException(response, jsonPayload);

            return response.Content;
        }

        public string GetSingleStoreInformation(string storeNumber)
        {
            var client = new RestClient(string.Format("https://staging.shoesensation.com/rest/V1/mwlocations/{0}", storeNumber));
            var request = new RestRequest() { Method = Method.Get };
            request.Timeout = TimeSpan.FromSeconds(10);
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Cookie", "PHPSESSID=22228ec25af3f936f8a111c3da132a9b");
            RestResponse response = client.Execute(request);

            return response.Content;
        }

        // Creates new Brick and Mortar Store in Magento
        public string CreateNewStoreInMagento(string jsonPayload)
        {
            var client = new RestClient("https://staging.shoesensation.com/rest/V1/mwlocations/create");
            var request = new RestRequest() { Method = Method.Post };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=bf68c7713e5a49f3a3b64a2af24efd5f; mage-messages=%5B%7B%22type%22%3A%22error%22%2C%22text%22%3A%22Invalid%20Form%20Key.%20Please%20refresh%20the%20page.%22%7D%5D; private_content_version=d2f9cbd9190900d783af6e84acf8b471");
            request.AddStringBody(jsonPayload, DataFormat.Json);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            ThrowException(response, jsonPayload);

            return response.Content;
        }

        // Retrieves a list of all Brick and Mortar stores on Magento
        public string GetAllStoreCodes()
        {
            var client = new RestClient("https://shoesensation.com/rest/V1/mwlocations");
            var request = new RestRequest() { Method = Method.Get };
            request.Timeout = TimeSpan.FromSeconds(10);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=61fc198266a23e5772f5831941a974af; mage-messages=%5B%7B%22type%22%3A%22error%22%2C%22text%22%3A%22Invalid%20Form%20Key.%20Please%20refresh%20the%20page.%22%7D%5D; private_content_version=d2f9cbd9190900d783af6e84acf8b471");
            // No body needed for GET; remove payload addition
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            ThrowException(response, request.Resource ?? request.ToString());

            return response.Content;
        }

        private void ThrowException(RestResponse response, string payload)
        {
            if (response.ErrorException != null)
            {
                throw new Exception(string.Format("Error Pulling Magento Data -- {0} {1}", response.ErrorException.Message, response.ErrorException));
            }
            // RestResponse.StatusCode defaults to 0 for no response; compare to HttpStatusCode.OK
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(string.Format("Error Pulling Magento Data -- {0} on {1}", response.StatusDescription, payload));
            }
        }

        // Creates the source location for new store
        public void CreateSourceLocation(string storeNumber)
        {
            var client = new RestClient(string.Format("https://staging.shoesensation.com/rest/V1/mwlocations/createSource/{0}", storeNumber));
            var request = new RestRequest() { Method = Method.Post };
            request.Timeout = TimeSpan.FromSeconds(10);
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=e47c8de20114cc6a5349445b3336315a; X-Magento-Vary=c58cc7336841735bf5ef13185766282824a9d073; mage-messages=%5B%7B%22type%22%3A%22error%22%2C%22text%22%3A%22Invalid%20Form%20Key.%20Please%20refresh%20the%20page.%22%7D%5D; private_content_version=89d9c386b32b8b913b517806b49af8b7");
            var body = "{\"is_transfer_products\": 0}";
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        // Assigns new source location to the new store
        public void AssignSourceLocation(string storeNumber)
        {
            var client = new RestClient(string.Format("https://staging.shoesensation.com/rest/V1/mwlocations/assignSource/{0}", storeNumber));
            var request = new RestRequest() { Method = Method.Post };
            request.Timeout = TimeSpan.FromSeconds(10);
            request.AddHeader("Authorization", "Bearer ");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=e47c8de20114cc6a5349445b3336315a; X-Magento-Vary=c58cc7336841735bf5ef13185766282824a9d073; mage-messages=%5B%7B%22type%22%3A%22error%22%2C%22text%22%3A%22Invalid%20Form%20Key.%20Please%20refresh%20the%20page.%22%7D%5D; private_content_version=89d9c386b32b8b913b517806b49af8b7");
            var body = string.Concat("{\"sourceCode\": \"", storeNumber, "\"}");
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
    }
}
