using System;
using System.Collections.Generic;

#nullable disable

namespace back_end.Entities
{
    public partial class Product
    {
        public string Title { get; set; }
        public int? Price { get; set; }
        public string Detail { get; set; }
        public string Image { get; set; }
        public string Category { get; set; }
        public int ProductId { get; set; }
    }
}
