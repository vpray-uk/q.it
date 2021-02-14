using Microsoft.EntityFrameworkCore;
using Q.It.Models;

namespace Q.It.Data
{
    public class QContext : DbContext
    {

        public QContext()
        {

        }
        public QContext(DbContextOptions<QContext> options) : base(options)
        {
        }

        public DbSet<Participant> Participants { get; set; }
        public DbSet<FastFinishAnswer> FastFinishAnswers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuestionChoice> QuestionChoices { get; set; }
        public DbSet<QuestionnaireVersionHistory> QuestionnaireVersionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SeedData();
        }

    }
}