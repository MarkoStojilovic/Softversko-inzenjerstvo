using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("CV")]
    public class CV
    {
        [Key]
        public int ID { get; set; }
        [MaxLength(150)]
        public string obrazovanje { get; set; }
        public int godina_iskustva { get; set; }
        [MaxLength(60)]
        public string email { get; set; }
        [MaxLength(60)]
        public string adresa {get; set; }
        [MaxLength(30)]
        public string telefon {get; set;}
        
        public string licni_opis {get; set; }

        public string jezici  {get; set; }

        public string tehnologije { get; set; }

        public string work_experience { get; set; } 
        
        public virtual Radnik Radnik { get; set; }

    }
}