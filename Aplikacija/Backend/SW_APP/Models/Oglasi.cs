using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Oglasi")]
    public class Oglasi
    {
        [Key]
        public int ID { get; set; }
        [MaxLength(120)]
        public string naziv {get; set; }
        [MaxLength(150)]
        public string tehnologija { get; set; }
       
        public string opis { get; set; }
       
        public string lokacija {get; set; }
       
        public int plata { get; set; }
       
        [MaxLength(2)]
        public string remote_work { get; set; }

        public DateTime vreme {get; set; }
       
        public virtual Korisnik Korisnik { get; set; }
        
    }
}