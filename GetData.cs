using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractDataFromPowerBIDataset
{
    internal class PowerBIData
    {
        public async Task<List<string>> getData()
        {
            //Store the following variables in a secure place like azure key vault
            string clientID = "YOUR APP ID";
            string clientSecret = "YOUR APP SECRET";
            string tenantID = "YOUT TENANT ID";
            string authority = $"https://login.microsoftonline.com/{tenantID}";

            var app = 
            ConfidentialClientApplicationBuilder.Create(clientID)
                                                .WithClientSecret(clientSecret)
                                                .WithAuthority(new Uri(authority))
                                                .Build();


            string[] scopes = new string[] { "https://analysis.windows.net/powerbi/api/.default" };

            var result = await app.AcquireTokenForClient(scopes)
                                  .ExecuteAsync();

            string accessToken = result.AccessToken;

            using (var client = new PowerBIClient(new Uri("https://api.powerbi.com"), new TokenCredentials(accessToken, "Bearer")))
            {

                string datasetId = "YOUR DATASET ID";

                string dax = "EVALUATE VALUES(Data[Category])";

                IList<DatasetExecuteQueriesQuery> query = new List<DatasetExecuteQueriesQuery>() { new DatasetExecuteQueriesQuery(dax) };

                var request = new DatasetExecuteQueriesRequest(query);

                var response = await client.Datasets.ExecuteQueriesAsync(datasetId, request);

                List<string> output = new List<string>();

                foreach (var data in response.Results[0].Tables[0].Rows)
                {
                    output.Add(data.ToString()!);
                }
                return output;
            }
        }
    }
}
