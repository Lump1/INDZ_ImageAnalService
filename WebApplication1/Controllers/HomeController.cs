﻿#region Usings

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

#endregion

namespace WebApplication1.Controllers
{
    public struct TabLink
    {
        public List<string> links;
    }

    public class HomeController : Controller
    {
        #region Configuration

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

        #endregion

        #region Views

        public IActionResult Index()
        {
            //await foreach(BlobItem blob in BlobContainerClient.GetBlobsAsync())
            //{
            //    var blockBlob = (CloudBlockBlob)blob;
            //    MemoryStream downloadStream = new MemoryStream();
            //    blockBlob.DownloadToStream(downloadStream);
            //}

            //ViewBag.Images = blobs;

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

        #endregion

        #region Images

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {

            
            if (image != null && image.Length > 0)
            {
                #region Blob Adding

                BlobClient blobClient = BlobContainerClient.GetBlobClient(image.FileName);

                if (!blobClient.Exists())
                    using (var stream = new MemoryStream())
                    {
                        await image.CopyToAsync(stream);

                        await blobClient.UploadAsync(new BinaryData(stream.ToArray()));                        
                    }

                #endregion

                #region Analisis

                Uri uri = await ImagesMethods.CreateServiceSASBlob(blobClient);
                string url = uri.AbsoluteUri;

                ImageAnalysis? analys;

                try
                {
                    analys = ComputerVisionMethods.AnalyzeImageAsync(visionClient, url).Result;

                }
                catch
                {
                    await blobClient.DeleteIfExistsAsync();

                    return View("Index", new ImageModelView(Description: "Oops! Something went wrong!"));
                }
                
                
                if(analys == null)
                {
                    await blobClient.DeleteIfExistsAsync();

                    return View("Index", new ImageModelView(Description: "Oops! Analysing error, try again later!"));
                }

                if(analys.Adult.AdultScore > 0.3)
                {
                    //await blobClient.DeleteIfExistsAsync();

                    string tagsString = "";

                    analys.Tags.ToList().ForEach(tag => tagsString += tag.Name.ToString() + ' ');


					return View("Index", new ImageModelView(Description: $"Adult score: " +
                            $"{analys.Adult.AdultScore.ToString()}\nTags: {tagsString}"
                        , uriUrl: url));
                }

                #endregion

                #region Db


                List<string> tagsList = analys.Tags.Where(tag => tag.Confidence > 0.5).Select(tag => tag.Name).ToList();

                string tags = "";
                foreach (string tag in tagsList)
                {
                    tags += tag + ' ';
                }

                await _context.Images.AddAsync(new Data.CosmosDb.Image(image.FileName, tags));

                await _context.SaveChangesAsync();


                #endregion

                return View("Index", new ImageModelView(url, analys.Description.Captions.First().Text));

            }
            else
            {
                ModelState.AddModelError("image", "Please select a file.");
                return View("Index", new ImageModelView());
            }
        }

        

        #endregion

        #region Errors

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion
    }
}