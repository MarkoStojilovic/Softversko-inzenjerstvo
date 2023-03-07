using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Zahtevi")]
    public class Zahtevi
    {
        [Key]
        public int ID { get; set; }
       
        public int status { get; set; }
        
        public virtual Korisnik ID_Posiljaoca { get; set; }
        
        public virtual Korisnik ID_Primaoca { get; set; }

        public virtual Oglasi Oglasi { get; set; }

    }
}