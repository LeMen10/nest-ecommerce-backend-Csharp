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
    public class SiteController : ControllerBase
    {
        public readonly web_apiContext _context;
        private readonly IConfiguration _config;

        public SiteController(web_apiContext ctx, IConfiguration config)
        {
            _context = ctx;
            _config = config;
        }

        [HttpGet("get-address")]
        public async Task<IActionResult> GetAddressUser()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string username = GetUserId();

            if (username == "") return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;

            var userResult = _context.Users.FirstOrDefault(u => u.UserId == userID);
            if (userResult.City == null) return Ok(new { message = "No Address" });
            return Ok(new { message = "success", userResult });

        }

        [HttpPost("update-address")]
        public async Task<IActionResult> UpdateAddress([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string username = GetUserId();

            if (username == "") return Unauthorized();
            var userResult = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = userResult.UserId;

            if (userResult == null) return NotFound();

            userResult.Phone = user.Phone;
            userResult.City = user.City;
            userResult.District = user.District;
            userResult.Ward = user.Ward;
            userResult.SpecificAddress = user.SpecificAddress;
            userResult.FullName = user.FullName;

            _context.Users.Update(userResult);
            await _context.SaveChangesAsync();

            userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userID);
            return Ok(new { message = "success" });
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchProduct([FromQuery] string query, [FromQuery] int page, [FromQuery] int limit)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string username = GetUserId();

            if (username == "") return Unauthorized();
            var userResult = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = userResult.UserId;

            if (userResult == null) return NotFound();

            int countSkip = (page - 1) * limit;

            var queryResult = _context.Products.Where(p => p.Title.Contains(query));

            double countProduct = queryResult.Count();
            countProduct = Math.Ceiling(countProduct / limit);

            var result = queryResult
                .OrderBy(x => 1)
                .Skip(countSkip)
                .Take(limit)
                .ToList();

            return Ok(new { message = "success", result , countProduct});
        }

        [HttpPost("product-checkout")]
        public async Task<IActionResult> ProductCheckout([FromBody] int[] dataIds)
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
