using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataProvider.Entities
{
    [Table("sale")]
    public class Sale
    {
        public int Id { get; set; }
        public int PId { get; set; }
        public decimal SalePrice { get; set; }
        public int SaleCount { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsFinished { get; set; }
    }
}
