using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    public class CV
    {
        //Saxat från kravspec: Information som ska finnas med är minst; Kompetenser, Utbildningar, Tidigare erfarenhet
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id{ get; set; }
        public virtual ICollection<CVUtbildning>? CVUtbildningar { get; set; } //ÄNDRAT till lista av utbildningar som pekar på Utbildning / CVUtbildning ?
        public virtual ICollection<CVErfarenhet>? CVErfarenheter { get; set; } //ÄNDRAT till lista av erfarenheter som pekar på CVErfarenhet 
        public virtual ICollection<CVKompetens>? CVKompetenser { get; set; }  //ÄNDRAT till lista av kompetenser som pekar CVKompetens ?

        //[ForeignKey(nameof(Projekt.Id))]
        //public virtual Projekt Projekt { get; set; }  //Navigation property, ska försöka konvertera ProjektID till ProjektNamn på projekt senare.  
        //HH

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        public string Picture { get; set; }

        public string ImagePath { get; set; } = "defaultProfilePic.jpg";

        // Lägg till ytterligare fält efter behov, profilbild är ett krav...
    }
}



