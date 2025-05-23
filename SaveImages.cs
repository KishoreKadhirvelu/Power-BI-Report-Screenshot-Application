using Microsoft.PowerBI.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Graph.Models;
using System.IO;
using System.IO.Pipes;
using Azure.Identity;

namespace SaveImagesToOneDrive
{
    internal class SaveImages
    {
        //Store the following variables in a secure place like azure key vault
        private const string clientId = "YOUR APP ID";
        private const string tenantId = "YOUR TENANT ID";
        private const string clientSecret = "YOUR APP SECRET";
        private string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
        private readonly GraphServiceClient graphClient;
        private string? DriveID { get; set; }
        public SaveImages()
        {
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var authProvider = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

            graphClient = new GraphServiceClient(authProvider, scopes);
        }

        public async Task InitializeDrive(string userEmail)
        {
            var drive = await graphClient.Users[userEmail].Drive.GetAsync();
            DriveID = drive?.Id;
        }


        public async Task DeleteFiles(string folderPath)
        {
            try
            {

                var folder = await graphClient
                    .Drives[DriveID]
                    .Items["Root"]
                    .Children[folderPath]
                    .GetAsync();

                var files = await graphClient
                    .Drives[DriveID]
                    .Items[folder!.Id]
                    .Children
                    .GetAsync();

                // Delete each item (file) in the folder
                foreach (var file in files!.Value!)
                {
                    await graphClient.Drives[DriveID].Items[file.Id].DeleteAsync();
                }

                Console.WriteLine("All files deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task UploadFile(string imagePath)

        {
            using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                await graphClient.Drives[DriveID]
                    .Items["root"]
                    .ItemWithPath(imagePath)
                    .Content
                    .PutAsync(fileStream);
            }

        }

    }
}
