using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Q.It.Models
{
    [Table("QuestionChoice")]
    public class QuestionChoice
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string ChoiceString { get; set; }

        [ForeignKey("Question")]
        [Required]
        public Guid OfQuestionId { get; set; }
        public virtual Question OfQuestion { get; set; }

    }
}
