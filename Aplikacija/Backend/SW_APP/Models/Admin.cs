using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Admin")]
    public class Admin
    {
        [Key]
        public int ID { get; set; }
        public virtual Korisnik Korisnik { get; set; }
        
        [MaxLength(30)]
        public string ime { get; set; }
        [MaxLength(30)]
        public string prezime { get; set; }
    }
}
