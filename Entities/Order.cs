using System;
using System.Collections.Generic;

#nullable disable

namespace back_end.Entities
{
    public partial class Order
    {
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public string Payment { get; set; }
        public int OrderId { get; set; }

        public virtual User User { get; set; }
    }
}
