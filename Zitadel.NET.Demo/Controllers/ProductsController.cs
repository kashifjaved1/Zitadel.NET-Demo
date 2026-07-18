using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zitadel.NET.Demo.Data.Entities;

namespace Zitadel.NET.Demo.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        static readonly List<Product> Products =
        [
            new Product { Id = 1, Name = "Laptop" },
        new Product { Id = 2, Name = "Mouse" }
        ];

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return Ok(Products);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(int id)
        {
            var product = Products.FirstOrDefault(x => x.Id == id);

            return product == null
                ? NotFound()
                : Ok(product);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(Product product)
        {
            Products.Add(product);

            return Ok(product);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, Product product)
        {
            var existing = Products.FirstOrDefault(x => x.Id == id);

            if (existing == null)
                return NotFound();

            existing.Name = product.Name;

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var existing = Products.FirstOrDefault(x => x.Id == id);

            if (existing == null)
                return NotFound();

            Products.Remove(existing);

            return NoContent();
        }
    }
}
