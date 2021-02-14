using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Q.It.Models
{
    [Table("QuestionnaireVersionHistory")]
    public class QuestionnaireVersionHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Version { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public virtual List<Question> Questions { get; set; }
    }
}
