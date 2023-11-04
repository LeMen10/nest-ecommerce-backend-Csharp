using back_end.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        public readonly web_apiContext _context;

        public ProductController(web_apiContext ctx)
        {
            _context = ctx;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _context.Products.ToList();
            return Ok(new { message = "success", products });
        }


        [HttpGet("product-detail/{id}")]
        public async Task<IActionResult> FindProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(new { message = "success", product });
        }

        [HttpPost("{id}/add-to-cart")]
        public async Task<IActionResult> AddToCart(int id, [FromBody] Cart cart)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addCart = new Cart
            {
                Quantity = cart.Quantity,
            };

            _context.Carts.Add(addCart);
            await _context.SaveChangesAsync();
            var products = _context.Products.ToList();
            return Ok(new { message = "success", products });
        }
    }
}
