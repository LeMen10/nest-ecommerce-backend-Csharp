using back_end.Entities;
using back_end.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly web_apiContext _context;
        private readonly IConfiguration _config;

        public AccountController(web_apiContext ctx, IConfiguration config)
        {
            _context = ctx;
            _config = config;
        }

        [HttpGet("get-username")]
        public IActionResult GetUserName()
        {
            string username = GetUserId();
            if (username == "") return Ok();

            return Ok(new { message = "success", username });
        }

        [HttpGet("get-items-cart")]
        public async Task<IActionResult> GetItemCart()
        {
            string username = GetUserId();
            if (username == "") return Ok();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var result = _context.Carts.Where(c => c.UserId == user.UserId).Count();

            return Ok(new { message = "success", result });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Kiểm tra valid của model (ví dụ: username, password, email không được rỗng)

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Username))
            {
                return BadRequest("Password is required");
            }

            bool checkUsername = _context.Users.Any(u => u.Username == user.Username);

            if (checkUsername) return BadRequest("Username has been registered");

            var account = new User
            {
                Username = user.Username,
                Email = user.Email,
                Password = BC.HashPassword(user.Password),
                Rule = "Người dùng"
            };

            _context.Users.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "success" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            // Kiểm tra valid của model (ví dụ: username, password, email không được rỗng
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var checkUser = _context.Users.SingleOrDefault(u => u.Username == user.Username);
            if (checkUser == null) return NotFound();

            var key = _config["Jwt:Key"];
            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signinCredential = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            //tao token
            var tokenSetUp = new JwtSecurityToken(
                issuer:_config["Jwt:Issuer"],
                audience:_config["Jwt:Audience"],
                expires:DateTime.Now.AddDays(2),
                signingCredentials:signinCredential,
                claims:claims
            );

            //sinh ra token với các thông số ở trên
            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenSetUp);

            // Trả về kết quả thành công
            return Ok(new { message = "success", accessToken });

        }

        [HttpGet("purchase")]
        public async Task<IActionResult> Purchase([FromQuery] string type)
        {
            string username = GetUserId();
            if (username == "") return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;
            string status = "";

            if (type == "noted") status = "Đang xử lý";
            else if (type == "cancelled") status = "Đã hủy";
            else if (type == "complete") status = "Hoàn thành";
            else if (type == "delivering") status = "Đang giao hàng";

            var result = _context.Orders
                .Where(o => o.UserId == userID)
                .Join(
                      _context.OrderDetails,
                      order => order.OrderId,
                      orderDetail => orderDetail.OrderId,
                      (order, orderDetail) => new { Order = order, OrderDetail = orderDetail }
                )
                .Where(combined => combined.OrderDetail.Status == status)
                .Join(
                      _context.Products,
                      combined => combined.OrderDetail.ProductId,
                      product => product.ProductId,
                      (combined, product) => new
                      {
                         combined.OrderDetail.OrderDetailId,
                         combined.OrderDetail.PaymentStatus,
                         combined.OrderDetail.Quantity,
                         combined.OrderDetail.Status,
                         combined.OrderDetail.Total,
                         title = product.Title,
                         image = product.Image,
                      }
                )
                .ToList();

            return Ok(new { message = "success", result });
        }

        [HttpPost("order-cancel/{id}")]
        public async Task<IActionResult> OrderCancel(int id)
        {
            string username = GetUserId();
            if (username == "") return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            int userID = user.UserId;

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null) orderDetail.Status = "Đã hủy"; orderDetail.PaymentStatus = null;
            _context.Update(orderDetail);
            await _context.SaveChangesAsync();
            return Ok(new { message = "success" });
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
