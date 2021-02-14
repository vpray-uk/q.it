using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Q.It.Models
{
    [Table("FastFinishAnswer")]
    public class FastFinishAnswer
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }
        public string Answer { get; set; }
        public virtual Question Question { get; set; }
    }
}
