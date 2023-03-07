using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Korisnik")]
    public class Korisnik
    {
        [Key]
        public int ID { get; set; }
        
        
        public string lozinka { get; set; }
        public byte[] salt_value {get; set; }

        [MaxLength(30)]
        public string korisnicko_ime { get; set; }
        [MaxLength(60)]
        public string email { get; set; }
        
        [MaxLength(10)]
        public string tip { get; set; }  

        public string grad { get; set; }

        public string imgPath {get;set;}

        
    }
}
