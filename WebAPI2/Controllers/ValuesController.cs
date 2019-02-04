using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DevTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebAPI2.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public ValuesController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var developerlist = await LookupDeveloper();
            foreach (var dev in developerlist)
            {
                //TODO: Do in a method and finish requirements
                dev.Skills.Where(x => x.Level >= 8 );
            }
            var result = developerlist;
            return new JsonResult(result);
        }      

        private async Task<List<Developer>> HttpConnection()
        {
            List<Developer> result;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:52460/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync("api/v1.0/Developer");
            
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsAsync<IEnumerable<Developer>>().Result;
                result = data.ToList();
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


    }
}
