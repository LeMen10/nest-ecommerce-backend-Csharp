using back_end.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        public readonly web_apiContext _context;

        public AdminController(web_apiContext ctx)
        {
            _context = ctx;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _context.Products.ToList();
            return Ok(new { message = "success", products });
        }

        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var addProduct = new Product
            {
                Title = product.Title,
                Price = product.Price,
                Detail = product.Detail,
                CategoryId = product.CategoryId,
                Image = "http://localhost:13395/image/" + product.Image, 
            };

            _context.Products.Add(addProduct);
            await _context.SaveChangesAsync();
            var products = _context.Products.ToList();
            return Ok(new { message = "success", products });
        }

        [HttpDelete("delete-product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            var products = _context.Products.ToList();
            return Ok(new { message = "success", products });
        }

        [HttpGet("find-product/{id}")]
        public async Task<IActionResult> FindProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(new { message = "success", product });
        }

        [HttpPut("edit-product/{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            var cate = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == updatedProduct.CategoryId);
            if (cate == null) return NotFound();

            product.Title = updatedProduct.Title;
            product.Price = updatedProduct.Price;
            product.CategoryId = updatedProduct.CategoryId;
            product.Detail = updatedProduct.Detail;
            product.Image = "http://localhost:13395/image/" + updatedProduct.Image;


            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var products = _context.Products.ToList();
            return Ok(new { message = "success", products });

        }

        [HttpGet("get-orders")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = _context.OrderDetails.ToList();
            return Ok(new { message = "success" });
        }
    }
}
