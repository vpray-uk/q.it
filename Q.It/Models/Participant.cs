using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Q.It.Models
{
    [Table("Participant")]
    public class Participant
    {
        [Key]
        public Guid Id { get; set; }

        public virtual Question CurrentQuestion { get; set; }
        public bool QuestionnaireDone { get; set; }
        public bool Answered { get; set; }

        [Required]
        public DateTime CreatedOn {get;set;}

        public DateTime? UpdatedOn { get; set; }

        public virtual List<Answer> Answers { get; set; }
    }
}
