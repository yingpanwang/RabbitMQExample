using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataProvider.Entities
{
    [Table("order")]
    public class Order
    {
        public int Id { get; set; }
        public int PId { get; set; }
        public int? SId { get; set; }
        public string UserId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
