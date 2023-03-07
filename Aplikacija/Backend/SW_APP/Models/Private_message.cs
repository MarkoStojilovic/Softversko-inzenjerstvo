using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Private_Message")]
    public class Private_Message
    {
        [Key]
        public int ID { get; set; }

        public string poruka { get; set; }
        
        [MaxLength(200)]
        public string naslov {get; set; } 
        
        public DateTime vreme_pristizanja { get; set; }
        
        public virtual Korisnik ID_Posiljaoca { get; set; }
        
        public virtual Korisnik ID_Primaoca { get; set; }
    }
}