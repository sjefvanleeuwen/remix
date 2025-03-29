using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemixHub.Shared.Models
{
    public class InstrumentType
    {
        [Key]
        public int InstrumentTypeId { get; set; }
        
        [Required, StringLength(50)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public int? ParentInstrumentTypeId { get; set; }
        
        // Navigation properties
        [ForeignKey("ParentInstrumentTypeId")]
        public virtual InstrumentType ParentInstrumentType { get; set; }
        
        public virtual ICollection<InstrumentType> SubInstrumentTypes { get; set; } = new List<InstrumentType>();
        
        public virtual ICollection<Stem> Stems { get; set; } = new List<Stem>();
    }
}
