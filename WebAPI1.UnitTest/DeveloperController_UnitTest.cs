using DevTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using WebAPI1.Controllers;
using Xunit;

namespace WebAPI1.UnitTest
{
    public class DeveloperController_UnitTest
    {
        DeveloperController _controller;

        public DeveloperController_UnitTest()
        {
            //Mock cache DI
            var cache = new MemoryCache(new MemoryCacheOptions());
            //Set mock list of developers on cache
            List<Developer> developers = Developer.FromJson(System.IO.File.ReadAllText("developers.mock.json"));
            cache.Set("ListOfDevelopers", developers);

            _controller = new DeveloperController(cache);
        }

        [Fact]
        public async void Get_WhenCalled_ReturnsOkResult()
        {
            //Get action from controller
            var okResult = await _controller.Get();
            //Assert if is JsonResult
            Assert.IsType<JsonResult>(okResult);
            //Assert if status code was ok
            Assert.True(((JsonResult)okResult).StatusCode == 200);
        }
    }
}
