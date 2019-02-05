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
                var developerlist = await LookupDeveloper();
                var result = FilterDeveloper(developerlist);
                var res = new JsonResult(result);
                res.StatusCode = 200;
                return res;
            }catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }

        private async Task<List<Developer>> HttpConnection()
        {
            List<Developer> result;

            HttpClient client = new HttpClient();
            var baseURL = _configuration.GetSection("WebAPI").GetSection("BaseURL").Value;
            client.BaseAddress = new Uri(baseURL);
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var endpoint = _configuration.GetSection("WebAPI").GetSection("GetAllDevelopers").Value;
            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsAsync<IEnumerable<Developer>>().Result;
                result = data != null ? data.ToList() : new List<Developer>(); 
            }
            else
            {
                //Something has gone wrong, handle it here
                result = new List<Developer>();
            }

            return result;
        }

        private async Task<List<Developer>> LookupDeveloper()
        {
            if (!_cache.TryGetValue("ListOfDevelopers", out List<Developer> developers))
            {
                developers = await HttpConnection();

                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(25), // cache will expire in 25 seconds
                    SlidingExpiration = TimeSpan.FromSeconds(5) // cache will expire if inactive for 5 seconds
                };

                _cache.Set("ListOfDevelopers", developers, options);
            }

            return developers;
        }

        private List<Developer> FilterDeveloper(List<Developer> developers)
        {
            var result = new List<Developer>();
            foreach (var dev in developers)
            {
                var skill = dev.Skills.Where(x => x.Level >= 8).FirstOrDefault();
                if(skill != null)
                {
                    dev.Skills.RemoveAll(x => x.Type != skill.Type);
                    result.Add(dev);
                }                
            }
            return result;
        }
    }
}
