using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Q.It.Models
{
    [Table("Question")]
    public class Question
    {
        [Key]
        public Guid Id { get; set; }

        public string QuestionTopic { get; set; }

        [Required]
        [ForeignKey("QuestionnaireVersionHistory")]
        public Guid VersionId { get; set; }
        [Required]
        public string QuestionString { get; set; }

        [Required]
        public int Order { get; set; }

        public virtual List<QuestionChoice> Choices { get; set; }

        public virtual QuestionnaireVersionHistory Version { get; set; }



    }
}
