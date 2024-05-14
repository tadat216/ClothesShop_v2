using ClothesShop.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.EF
{
    [Table("Parameter")]
    public class Parameter
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Parameter() { this.Id = IdGenerator.GetId(db.Parameters); }
        [Key]
        public string Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}