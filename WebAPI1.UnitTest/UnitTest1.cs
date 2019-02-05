using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using WebAPI1.Controllers;
using Xunit;

namespace WebAPI1.UnitTest
{
    public class UnitTest1
    {
        DeveloperController _controller;

        public UnitTest1()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            _controller = new DeveloperController(cache);
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
