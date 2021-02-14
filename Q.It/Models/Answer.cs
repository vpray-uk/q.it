using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Q.It.Models
{
    [Table("Answer")]
    public class Answer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public virtual Question AnsweredQuestion { get; set; }
        [Required]
        public virtual Participant AnsweringParticipant { get; set; }
        [Required]
        public string AnswerString { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
    }
}
