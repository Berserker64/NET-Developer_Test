using System;
using Xunit;
using WebAPI2.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using DevTest;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace WebAPI2.UnitTest
{
    public class UnitTest1
    {
        ValuesController _controller;

        public UnitTest1()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
            _controller = new ValuesController(cache, config);
        }
             
        [Fact]
        public async void Get_WhenCalled_ReturnsOkResult()
        {           
            var x = await _controller.Get();
            IActionResult okResult = x;
                        
            Assert.IsType<JsonResult>(okResult);
            Assert.True(((JsonResult)okResult).StatusCode == 200);
        }

    }
}
