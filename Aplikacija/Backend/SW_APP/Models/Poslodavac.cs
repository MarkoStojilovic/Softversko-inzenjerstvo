using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Poslodavac")]
    public class Poslodavac
    {
        [Key]
        public int ID { get; set; } 

        public virtual Korisnik Korisnik { get; set; }

       [MaxLength(13)]
        public string tip_poslodavca { get; set; }
        [MaxLength(50)]
        public string kontakt { get; set; }
        [MaxLength(30)]
        public string ime { get; set; }
        [MaxLength(30)]
        public string prezime { get; set; }
     
        public string lokacija_firme { get; set; }
        [MaxLength(120)]
        public string naziv_firme { get; set; }

    }
}