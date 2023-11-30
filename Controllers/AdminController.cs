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

        [HttpGet("get-products")]
        public IActionResult GetProduct([FromQuery] int page, [FromQuery] int limit)
        {
            int countSkip = (page - 1) * limit;
            var products = _context.Products.Where(p => p.IsDeleted != true)
                .Join(
                    _context.Categories,
                    product => product.CategoryId,
                    category => category.CategoryId,
                    (product, category) => new {
                        product.Title,
                        product.Image,
                        product.Price,
                        product.ProductId,
                        category.Cate
                    }
                )
                .Select(p => new { p.Title, p.Image, p.Price, p.ProductId, p.Cate })
                .Skip(countSkip).Take(limit).ToList();

            double count = _context.Products.Count();
            var countProduct = Math.Ceiling(count / limit);

            return Ok(new { message = "success", products, countProduct });
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
            var cate = await _context.Categories.FindAsync(product.CategoryId);
            if (cate == null) return NotFound();

            cate.Count += 1;
            _context.Categories.Update(cate);
            await _context.SaveChangesAsync();

            return Ok(new { message = "success" });
        }

        [HttpDelete("delete-product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "success" });
        }

        [HttpGet("find-product/{id}")]
        public async Task<IActionResult> FindProduct(int id)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                .Join(
                    _context.Categories,
                    p => p.CategoryId,
                    c => c.CategoryId,
                    (p, c) => new { c.CategoryId, c.Cate, p.Detail, p.Image, p.Title, p.Price, p.ProductId }
                );

            if (product == null) return NotFound();

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

        [HttpGet("trash-products")]
        public IActionResult TrashProduct([FromQuery] int page, [FromQuery] int limit)
        {
            var query = _context.Products
                .Where(p => p.IsDeleted != false)
                .Join(
                    _context.Categories,
                    product => product.CategoryId,
                    category => category.CategoryId,
                    (product, category) => new {
                        product.Title, 
                        product.Image, 
                        product.Price, 
                        product.ProductId,
                        category.Cate
                    }
                )
                .Select(p => new { p.Title, p.Image, p.Price, p.ProductId, p.Cate });

            int countSkip = (page - 1) * limit;
            var products = query.Skip(countSkip).Take(limit).ToList();

            double count = query.Count();
            var countProduct = Math.Ceiling(count / limit);

            return Ok(new { message = "success", products, count });
        }

        [HttpPut("restore-product/{id}")]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            product.IsDeleted = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "success" });
        }

        [HttpGet("get-count-product-deleted")]
        public IActionResult GetNumberProductDeleted()
        {
            var count = _context.Products.Where(u => u.IsDeleted == true).Count();

            return Ok(new { message = "success", count });
        }


        [HttpGet("get-orders")]
        public IActionResult GetOrders([FromQuery] int page, [FromQuery] int limit)
        {
            var query = _context.Orders
                .Join(
                     _context.OrderDetails,
                     order => order.OrderId,
                     orderDetail => orderDetail.OrderId,
                     (order, orderDetail) => new {
                         Order = order,
                         OrderDetail = orderDetail
                     }
                )
                .Join(
                    _context.Users,
                    result => result.Order.UserId,
                    user => user.UserId,
                    (result, user ) => new
                    {
                        result.OrderDetail.OrderDetailId,
                        result.OrderDetail.Status,
                        result.OrderDetail.Total,
                        result.OrderDetail.PaymentStatus,
                        result.Order.CreateDate,
                        user.FullName
                    }
                )
                .Select(rs => new
                {
                    rs.OrderDetailId,
                    rs.Status,
                    rs.Total,
                    rs.PaymentStatus,
                    CreateDate = rs.CreateDate.ToString("yyyy-MM-dd"), // Chỉ lấy phần ngày
                    rs.FullName
                });

            int countSkip = (page - 1) * limit;
            var orderDetails = query.Skip(countSkip).Take(limit).ToList();

            double count = query.Count();
            var countProduct = Math.Ceiling(count / limit);

            return Ok(new { message = "success", orderDetails, countProduct });
        }

        [HttpPut("update-status-order/{orderDetailId}")]
        public async Task<IActionResult> UpdateStatusOrder(int orderDetailId, [FromBody] OrderDetail orderDetail)
        {
            var query = await _context.OrderDetails.FirstOrDefaultAsync(or => or.OrderDetailId == orderDetailId);

            if (query == null) return NotFound();
            if (orderDetail.Status == "Hoàn thành") query.PaymentStatus = "Đã thanh toán";
            query.Status = orderDetail.Status;

            _context.OrderDetails.Update(query);
            await _context.SaveChangesAsync();

            return Ok(new { message = "success" });

        }

        [HttpGet("get-number-order")]
        public IActionResult GetNumberOrder()
        {
            var all = _context.OrderDetails.Count();
            var processing = _context.OrderDetails.Where(or => or.Status == "Đang xử lý").Count();
            var delivering = _context.OrderDetails.Where(or => or.Status == "Đang giao hàng").Count();
            var complete = _context.OrderDetails.Where(or => or.Status == "Hoàn thành").Count();
            return Ok(new { message = "success", all, processing, delivering, complete });
        }



        [HttpGet("get-users")]
        public IActionResult GetUsers([FromQuery] int page, [FromQuery] int limit)
        {
            int countSkip = (page - 1) * limit;
            var users = _context.Users.Where(u => u.IsDeleted != true).OrderBy(x => 1).Skip(countSkip).Take(limit).ToList();

            double count = _context.Users.Count();
            var countProduct = Math.Ceiling(count / limit);

            return Ok(new { message = "success", users });
        }

        [HttpPut("restore-user/{id}")]
        public async Task<IActionResult> RestoreUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            user.IsDeleted = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var users = _context.Users.Where(u => u.IsDeleted == true).ToList();
            return Ok(new { message = "success", users });
        }

        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound();

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "success" });
        }

        [HttpGet("trash-users")]
        public IActionResult TrashUsers([FromQuery] int page, [FromQuery] int limit)
        {
            var query = _context.Users.Where(p => p.IsDeleted == true).ToList();

            int countSkip = (page - 1) * limit;
            var users = query.Skip(countSkip).Take(limit).ToList();

            double count = query.Count;
            var countUser = Math.Ceiling(count / limit);

            return Ok(new { message = "success", users, countUser });
        }

        [HttpGet("get-count-user-deleted")]
        public IActionResult GetNumberUserDeleted()
        {
            var count = _context.Users.Where(u => u.IsDeleted == true).Count();

            return Ok(new { message = "success", count });
        }

        
    }
}
