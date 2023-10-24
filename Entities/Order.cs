using System;
using System.Collections.Generic;

#nullable disable

namespace back_end.Entities
{
    public partial class Order
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Payment { get; set; }

        public virtual User User { get; set; }
    }
}
