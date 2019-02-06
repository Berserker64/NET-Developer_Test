using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevTest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebAPI1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public DeveloperController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        // GET: api/Developer
        [MapToApiVersion("1.0")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                //Check on cache memory list if is expired list will get again from developer.json file
                var developerlist = await LookupDevelopers();
                var response = new JsonResult(developerlist);
                response.StatusCode = 200;
                return response;
                
            }catch (Exception e){
                return new StatusCodeResult(500);
            }
        }

        private async Task<List<Developer>> LookupDevelopers()
        {
            //check if exist in cache memory
            if (!_cache.TryGetValue("ListOfDevelopers", out List<Developer> developers))
            {
                // get the list from developer.json file
                developers = Developer.FromJson(await System.IO.File.ReadAllTextAsync("developers.json"));

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
    }

}
