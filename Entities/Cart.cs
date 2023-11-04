using System;
using System.Collections.Generic;

#nullable disable

namespace back_end.Entities
{
    public partial class Cart
    {
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }

        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
}
