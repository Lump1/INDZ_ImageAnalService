using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using WebApplication1.Data;
using WebApplication1.Data.CosmosDb;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;
        private readonly IWebHostEnvironment _env;

        private BlobServiceClient BlobServiceClient;
        private BlobContainerClient BlobContainerClient;

        private Data.CosmosDb.CosmosDbContext _context;


        public SearchController(ILogger<SearchController> logger, IWebHostEnvironment env, AzureKeys azureKeys)
        {
            _logger = logger;
            _env = env;

            BlobServiceClient = new BlobServiceClient(azureKeys.BLOB_SERVICE_KEY);
            BlobContainerClient = BlobServiceClient.GetBlobContainerClient("home");

            _context = new Data.CosmosDb.CosmosDbContext();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string inputedRes = "", int page = 1)
        {
            List<string> imagesName = new List<string>();
			int limit = 30 * page;

			if (inputedRes == null || inputedRes == "" || inputedRes == string.Empty)
            {
				

				imagesName = _context.Images.ToListAsync().Result
							.Where(image => image.Id > limit - 30 && image.Id < limit)
							.Select(image => image.Name)
                            .ToList();

				return View("Search", new Models.SearchModel(SelectUrls(imagesName)));
			}


            var inputedResArr = inputedRes.Split(' ');
            var imgList = await _context.Images.ToListAsync();

            foreach(var img in imgList)
            {
                for (int i = 0; i < inputedResArr.Length; i++)
                    if (img.Tags.Split(' ').FirstOrDefault(tag => tag == inputedResArr[i]) != null)
                    {
                        imagesName.Add(img.Name);
                        continue;
					}
                        
                        
			}

            return View("Search", new WebApplication1.Models.SearchModel(SelectUrls(imagesName)));
        }


		private IEnumerable<string> SelectUrls(IEnumerable<string> names)
		{
			return names.Select(name =>
			{
				BlobClient blobClient = BlobContainerClient.GetBlobClient(name);

				Uri uri = ImagesMethods.CreateServiceSASBlob(blobClient).Result;
				return uri.AbsoluteUri;
			});
		}
	}
}
