using System;
using Xunit;
using WebAPI2.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using DevTest;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Linq;

namespace WebAPI2.UnitTest
{
    public class ValuesController_UnitTest
    {
        ValuesController _controller;

        public ValuesController_UnitTest()
        {
            //Mock cache and config DI
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();
            //Set mock list of developers on cache
            List<Developer> developers = Developer.FromJson(System.IO.File.ReadAllText("developers.mock.json"));
            cache.Set("ListOfDevelopers", developers);

            _controller = new ValuesController(cache, config);
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

        [Fact]
        public void Test_FilterDeveloper()
        {
            //Set mock list of developers on cache
            List<Developer> developers = Developer.FromJson(System.IO.File.ReadAllText("developers.mock.json"));
            var devresult = _controller.FilterDeveloper(developers);
            //Validate filter criteria of the result if is ok
            var exists = devresult.All(x => x.Skills.GroupBy(p => p.Type).Count() == 1   //Validate that is only one group of Skill Type for each Developer
                                                            && x.Skills.Exists(p => p.Level > 7));        //Validate that exist at least one skill above of 7
            //Assert result
            Assert.True(exists);

        }

        [Fact]
        public async void Test_Integration_end2end()
        {
            //Get action from controller
            var okResult = await _controller.Get();
            //Assert if is JsonResult
            Assert.IsType<JsonResult>(okResult);
            //Assert if status code was ok
            Assert.True(((JsonResult)okResult).StatusCode == 200);
            //get values
            var devresult = ((JsonResult)okResult).Value;
            //Validate filter criteria of the result if is ok
            var exists = ((List<Developer>)devresult).All(x => x.Skills.GroupBy(p => p.Type).Count() == 1   //Validate that is only one group of Skill Type for each Developer
                                                            && x.Skills.Exists(p => p.Level > 7));        //Validate that exist at least one skill above of 7
            //Assert result
            Assert.True(exists);
        }
    }
}
