using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.VisualBasic.FileIO;
using static System.Net.WebRequestMethods;
using System.Text;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.Collections.Generic;
using ExtractDataFromPowerBIDataset;
using System.Text.Json;
using Newtonsoft.Json;
using SaveImagesToOneDrive;
using Microsoft.Graph.Models;


class PowerBIReportScreenshots
    {
    static async Task Main(string[] args)
    {
        try
        {
            var getPowerBIData = new PowerBIData();
            List<string> categories = await getPowerBIData.getData(); //Get data from Power BI dataset

            var folderPath = "Screenshots";

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            Directory.CreateDirectory(folderPath);

            var images = new SaveImages();
            await images.InitializeDrive("YOUR EMAIL ID"); //This is the account under which images will be stored
            await images.DeleteFiles(folderPath);

            new DriverManager().SetUpDriver(new ChromeConfig()); //This will install chrome driver version that is compatible with chrome browser 
            var driver = new ChromeDriver();

            string initialUrl = "https://app.powerbi.com/groups/WorkspaceID/reports/ReportID?";
            driver.Navigate().GoToUrl(initialUrl);
            driver.Manage().Window.Maximize();
            Thread.Sleep(4000);

            IWebElement email = driver.FindElementByCssSelector("#email");
            email.SendKeys("YOUR EMAIL ID");
            email.SendKeys(Keys.Return);
            Thread.Sleep(5000);

            IWebElement pwd = driver.FindElementByCssSelector("#i0118");
            pwd.SendKeys("YOUR PASSWORD"); //Strongly recommend to store this in a secure place like azure key vault
            pwd.SendKeys(Keys.Return);
            Thread.Sleep(3000);

            IWebElement staySignedIn = driver.FindElementByCssSelector("#idSIButton9");
            staySignedIn.SendKeys(Keys.Return);
            Thread.Sleep(15000);

            var index = 1;
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Saving Screenshots...");

            foreach (string item in categories)
            {
                MyCategory category = JsonConvert.DeserializeObject<MyCategory>(item)!;

                string url = $"https://app.powerbi.com/groups/WorkspaceID/reports/ReportID?experience=power-bi&filter=Data%2FCategory%20eq%20%27{category.Category}%27";

                driver.Navigate().GoToUrl(url);
                Thread.Sleep(12000);

                IWebElement element = driver.FindElementByCssSelector("#pvExplorationHost > div > div > exploration > div > explore-canvas > div > div.canvasFlexBox > div > div.displayArea.disableAnimations.fitToPage");
                
                Screenshot screenshot = (element as ITakesScreenshot)!.GetScreenshot();

                Console.WriteLine($"{index}. {category.Category}");

                var filePath = folderPath + "\\" + category.Category + ".png";

                screenshot.SaveAsFile(filePath, ScreenshotImageFormat.Png);

                await images.UploadFile(filePath);

                index++;
            }
            driver.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured. Please close the application and try later");
            Console.WriteLine("Error Message is : " + ex);
            Console.ReadKey();
        }
    }

}

public class MyCategory
{
    [JsonProperty("Data[Category]")]
    public string? Category { get; set; }
}