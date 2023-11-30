using back_end.Entities;
using back_end.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        public readonly web_apiContext _context;
        private readonly IConfiguration _config;

        public PaymentController(web_apiContext ctx, IConfiguration config)
        {
            _context = ctx;
            _config = config;
        }

        [HttpPost("save-order")]
        public async Task<IActionResult> SaveOrder([FromBody] Entities.Order order)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                string username = GetUserId();

                if (username == "") return Unauthorized();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                int userID = user.UserId;

                string payment = order.Payment;

                var newOrder = new Entities.Order
                {
                    UserId = userID,
                    Payment = payment,
                };

                _context.Orders.Add(newOrder);
                _context.SaveChanges();

                int OrderId = newOrder.OrderId;

                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    var orderDetails = order.OrderDetails.Select(od => new OrderDetail
                    {
                        OrderId = OrderId,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        Status = od.Status,
                        PaymentStatus = od.PaymentStatus,
                        Total = od.Total
                    });

                    _context.OrderDetails.AddRange(orderDetails);
                    _context.SaveChanges();
                }

                return Ok(new { message = "success", OrderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error: {ex.Message}" });
            }

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
