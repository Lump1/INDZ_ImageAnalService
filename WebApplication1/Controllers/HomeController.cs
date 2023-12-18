#region Usings

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection.Metadata;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using WebApplication1.Data.CosmosDb;
using System.Security.Policy;

#endregion

namespace WebApplication1.Controllers
{
    public struct TabLink
    {
        public List<string> links;
    }

    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        private string comVisKey = "d1e0d09e976c41dd8d88906e2f90b767";
        private string comVisUrl = "https://comvisiongur.cognitiveservices.azure.com/";

        private BlobServiceClient BlobServiceClient;
        private BlobContainerClient BlobContainerClient;

        private ComputerVisionClient visionClient;
        private Data.CosmosDb.CosmosDbContext _context;


        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;

            BlobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=gureev;AccountKey=ZiS2bpWpXNqxvUbun2GFYi1AEVCZxDwaokAFcH3VjPZmRUOopJJgeTBRJ79alAKMSfd/s4ZiDJUQ+AStMIlH+g==;EndpointSuffix=core.windows.net");
            BlobContainerClient = BlobServiceClient.GetBlobContainerClient("home");
            BlobContainerClient.CreateIfNotExistsAsync().Wait();

            visionClient = ComputerVisionMethods.Authenticate(comVisKey, comVisUrl);

            _context = new Data.CosmosDb.CosmosDbContext();

            _init(_context);
		}

        public static async void _init(CosmosDbContext _context)
        {
			var lastItem = await _context.Images.OrderBy(image => image.Id).LastAsync();
			Data.CosmosDb.Image._init(lastItem.Id);
		}

        public IActionResult Index()
        {
            return View(new ImageModelView());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
        public ViewResult Result()
        {
            return View("Result");
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {      
            if (image != null && image.Length > 0)
            {
                BlobClient blobClient = BlobContainerClient.GetBlobClient(image.FileName);

                await ImagesMethods.BlobUploadAsync(blobClient, image);

                Uri uri = await ImagesMethods.CreateServiceSASBlob(blobClient);
                string url = uri.AbsoluteUri;

                ImageAnalysis? analys;
                try
                {
                    analys = await ComputerVisionMethods.AnalyzeImageAsync(visionClient, url);
                }
                catch
                {
                    await blobClient.DeleteIfExistsAsync();
                    return View("Index", new ImageModelView(Description: "Oops! Something went wrong!"));
                }

                var checkResult = await CheckAnalysAsync(analys, blobClient);
                if(checkResult != null)
                {
                    return checkResult;
                }


                Dictionary<string, double> TagsAndConfidence = new Dictionary<string, double>();

                IEnumerable<ImageTag> tagsList = analys.Tags.Where(tag => tag.Confidence > 0.5);

                string tags = "";
                foreach (var tag in tagsList)
                {
                    tags += tag.Name + ' ';
                    TagsAndConfidence.Add(tag.Name, tag.Confidence);
                }

                await _context.Images.AddAsync(new Data.CosmosDb.Image(image.FileName, tags));
                await _context.SaveChangesAsync();

                return View("Result", new ImageModelView(url, analys.Description.Captions.First().Text, TagsAndConfidence));

            }
            else
            {
                ModelState.AddModelError("image", "Please select a file.");
                return View("Index", new ImageModelView());
            }
        }

        

        private async Task<ViewResult?> CheckAnalysAsync(ImageAnalysis? analysis, BlobClient blobClient)
        {
            if (analysis == null)
            {
                await blobClient.DeleteIfExistsAsync();

                return View("Index", new ImageModelView(Description: "Oops! Analysing error, try again later!"));
            }

            if (analysis.Adult.AdultScore > 0.3)
            {
                await blobClient.DeleteIfExistsAsync();

                string tagsString = "";

                analysis.Tags.ToList().ForEach(tag => tagsString += tag.Name.ToString() + ' ');


                return View("Index", new ImageModelView(Description: $"Oops! Image is 18+, take another"));
            }

            return null;
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}