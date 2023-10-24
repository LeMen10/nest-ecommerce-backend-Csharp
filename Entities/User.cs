using System;
using System.Collections.Generic;

#nullable disable

namespace back_end.Entities
{
    public partial class User
    {
        public User()
        {
            Orders = new HashSet<Order>();
        }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Rule { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
