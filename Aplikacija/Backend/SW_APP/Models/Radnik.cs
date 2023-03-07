using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Radnik")]
    public class Radnik
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