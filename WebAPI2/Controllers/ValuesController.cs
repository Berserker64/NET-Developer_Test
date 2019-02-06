using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DevTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace WebAPI2.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private IConfiguration _configuration;
        public ValuesController(IMemoryCache memoryCache, IConfiguration iConfig)
        {
            _cache = memoryCache;
            _configuration = iConfig;
        }

        // GET api/values
        [MapToApiVersion("1.0")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {            
            try
            {
                //Check on cache memory list if is expired list will get again from WebAPI1
                var developerlist = await LookupDevelopers();
                //Filter developerlist with skills level 8 or more and the same type
                var result = FilterDeveloper(developerlist);
                //perform response
                var response = new JsonResult(result);
                response.StatusCode = 200;
                return response;
            }catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }

        private async Task<List<Developer>> HttpConnection()
        {
            List<Developer> result;
            
            HttpClient client = new HttpClient();
            //Get URL from appsettings.json
            var baseURL = _configuration.GetSection("WebAPI").GetSection("BaseURL").Value;
            client.BaseAddress = new Uri(baseURL);
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Get Endpoint of the Api from appsettings.json
            var endpoint = _configuration.GetSection("WebAPI").GetSection("GetAllDevelopers").Value;
            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                //return response as a list 
                var data = response.Content.ReadAsAsync<IEnumerable<Developer>>().Result;
                result = data != null ? data.ToList() : new List<Developer>(); 
            }
            else
            {                
                result = new List<Developer>();
            }

            return result;
        }

        private async Task<List<Developer>> LookupDevelopers()
        {
            //check if exist in cache memory
            if (!_cache.TryGetValue("ListOfDevelopers", out List<Developer> developers))
            {
                //get the list from WebAPI1
                developers = await HttpConnection();

                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(25), // cache will expire in 25 seconds
                    SlidingExpiration = TimeSpan.FromSeconds(5) // cache will expire if inactive for 5 seconds
                };
                //save it on cache memory
                _cache.Set("ListOfDevelopers", developers, options);
            }

            return developers;
        }

        [NonAction]
        public List<Developer> FilterDeveloper(List<Developer> developers)
        {
            var result = new List<Developer>();
            foreach (var dev in developers)
            {
                //Get the first or defualt skill that is level 8 or more
                var skill = dev.Skills.Where(x => x.Level >= 8).FirstOrDefault();
                if(skill != null)
                {
                    //if the skill exist of level 8 or more, then remove all the skill that are not of the same type
                    dev.Skills.RemoveAll(x => x.Type != skill.Type);
                    //add dev on the result list
                    result.Add(dev);
                }                
            }
            return result;
        }
    }
}
