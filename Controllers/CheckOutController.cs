using back_end.Entities;
using back_end.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutController : ControllerBase
    {
        public readonly web_apiContext _context;
        private readonly IConfiguration _config;

        public CheckOutController(web_apiContext ctx, IConfiguration config)
        {
            _context = ctx;
            _config = config;
        }


        [HttpPost("product-checkout")]
        public async Task<IActionResult> ProductInCheckout([FromBody] int[] dataIds)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string username = GetUserId();

            if (username == "") return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;

            var carts = _context.Carts.Where(item => dataIds.Contains(item.CartId)).ToList();

            var result = carts.Join(
                _context.Products,
                c => c.ProductId,
                p => p.ProductId,
                (c, p) => new { c.UserId, c.ProductId, c.CartId, c.Quantity, p.Title, p.Price, p.Image });

            return Ok(new { message = "success", result });
        }

        private string GetUserId()
        {
            string token = HttpContext.Request.Headers["Authorization"];
            token = token.Substring(7);
            string secretKey = _config["Jwt:Key"];

            if (token == "undefined") return "";

            string username = VeryfiJWT.GetUsernameFromToken(token, secretKey);
            return username;
        }
    }

    
}
