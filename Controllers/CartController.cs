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
    public class CartController : ControllerBase
    {
        public readonly web_apiContext _context;
        private readonly IConfiguration _config;

        public CartController(web_apiContext ctx, IConfiguration config)
        {
            _context = ctx;
            _config = config;
        }



        [HttpGet("get-cart")]
        public async Task<IActionResult> GetCart()
        {
            string token = HttpContext.Request.Headers["Authorization"];
            token = token.Substring(7);
            string secretKey = _config["Jwt:Key"];

            if (token == "undefined")
            {
                return BadRequest();
            }

            string username = VeryfiJWT.GetUsernameFromToken(token, secretKey);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;

            var carts = await _context.Carts.Where(c => c.UserId == userID)
                .Select(c => new { c.UserId, c.ProductId, c.CartId, c.Quantity }).ToListAsync();

            var result = carts.Join(
                _context.Products,
                c => c.ProductId,
                p => p.ProductId,
                (c, p) => new { c.UserId, c.ProductId, c.CartId, c.Quantity, p.Title, p.Price, p.Image });

            if (result != null)
            {
                return Ok(new { message = "success", result });
            }

            return NotFound();
        }

        [HttpPost("{id}/add-to-cart")]
        public async Task<IActionResult> AddToCart(int id, [FromBody] Cart cart)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string token = HttpContext.Request.Headers["Authorization"];
            token = token.Substring(7);
            string secretKey = _config["Jwt:Key"];

            if (token == "undefined")
            {
                return BadRequest();
            }

            string username = VeryfiJWT.GetUsernameFromToken(token, secretKey);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;
            int productID = id;
            int quantity = (int)cart.Quantity;

            var cartOfUser = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userID && c.ProductId == productID);

            if (cartOfUser == null)
            {
                var cartItem = new Cart
                {
                    UserId = userID,
                    ProductId = productID,
                    Quantity = quantity
                };
                _context.Carts.Add(cartItem);
                await _context.SaveChangesAsync();
                return Ok(new { message = "success", cartOfUser });
            }
            else
            {
                var query = $"UPDATE Carts SET Quantity = {quantity + cartOfUser.Quantity} WHERE CartID = {cartOfUser.CartId}";
                await _context.Database.ExecuteSqlRawAsync(query);

                return Ok(new { message = "successes" });
            }
        }

        [HttpPost("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] Cart cart)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string token = HttpContext.Request.Headers["Authorization"];
            token = token.Substring(7);
            string secretKey = _config["Jwt:Key"];

            if (token == "undefined")
            {
                return BadRequest("hihi");
            }

            string username = VeryfiJWT.GetUsernameFromToken(token, secretKey);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;
            int cartID = cart.CartId;
            var cartOfUser = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userID && c.CartId == cartID);

            cartOfUser.Quantity = cart.Quantity;

            _context.Carts.Update(cartOfUser);
            await _context.SaveChangesAsync();

            var carts = await _context.Carts.Where(c => c.UserId == userID)
                .Select(c => new { c.UserId, c.ProductId, c.CartId, c.Quantity }).ToListAsync();

            var result = carts.Join(
                _context.Products,
                c => c.ProductId,
                p => p.ProductId,
                (c, p) => new { c.UserId, c.ProductId, c.CartId, c.Quantity, p.Title, p.Price, p.Image });

            return Ok(new { message = "successes", result });
        }

        [HttpPost("{id}/delete-cart-item")]
        public async Task<IActionResult> DeleteCartItem(int id, [FromBody] Cart cart)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string token = HttpContext.Request.Headers["Authorization"];
            token = token.Substring(7);
            string secretKey = _config["Jwt:Key"];

            if (token == "undefined")
            {
                return BadRequest();
            }

            string username = VeryfiJWT.GetUsernameFromToken(token, secretKey);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;

            return Ok(new { message = "success" });
        }
    }
}
