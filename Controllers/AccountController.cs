using back_end.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly web_apiContext _context;

        public AccountController(web_apiContext ctx)
        {
            _context = ctx;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Users.ToList());
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Kiểm tra valid của model (ví dụ: username, password, email không được rỗng)

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Username))
            {
                return BadRequest("Password is required");
            }

            bool checkUsername = _context.Users.Any(u => u.Username == user.Username);

            if (checkUsername)
            {
                return BadRequest("Username has been registered");
            }

            // Tạo một đối tượng Account từ model
            var account = new User
            {
                Username = user.Username,
                Email = user.Email,
                Password = BC.HashPassword(user.Password)
            };

            // Lưu thông tin tài khoản vào cơ sở dữ liệu
            _context.Users.Add(account);
            await _context.SaveChangesAsync();

            // Trả về kết quả thành công
            return Ok(new { message = "Đăng ký thành công" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {

            // Kiểm tra valid của model (ví dụ: username, password, email không được rỗng)

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // truy vấn cơ sở dữ liệu để kiểm tra thông tin đăng nhập
            // đây là ví dụ đơn giản, bạn cần triển khai phương thức này theo cách thích hợp với ứng dụng của bạn
            var checkUser = _context.Users.SingleOrDefault(u => u.Username == user.Username);

            if (user == null)
            {
                return NotFound();
            }

            // kiểm tra mật khẩu
            //bool ispasswordvalid = comparepassword(password, user.Password);


            // Trả về kết quả thành công
            return Ok("Đăng ký thành công");
        }

        //private bool authenticateuser(string username, string password)
        //{
            // truy vấn cơ sở dữ liệu để kiểm tra thông tin đăng nhập
            // đây là ví dụ đơn giản, bạn cần triển khai phương thức này theo cách thích hợp với ứng dụng của bạn
           // var user = _context.users.singleordefault(u => u.username == username);

           // if (user == null)
            //////{
               /// return false; // không tìm thấy người dùng với tên người dùng đã nhập
            //}

            // kiểm tra mật khẩu
          //  bool ispasswordvalid = comparepassword(password, user.password);

            ////return ispasswordvalid;
        //}

        private string EncryptPassword(string password)
        {
            throw new NotImplementedException();
        }

        private void SaveAccount(User account)
        {
            throw new NotImplementedException();
        }

        private bool IsAccountExists(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
