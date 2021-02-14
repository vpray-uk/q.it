using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Q.It.Data;
using Q.It.Models;
using Q.It.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Q_It.UnitTests
{
    public class ServiceTests
    {
        private readonly IQuestionnaireService QuestionnaireService;
        private readonly ILogger<QuestionnaireService> Logger;
        private readonly QContext Context;
        public ServiceTests()
        {
            //create In Memory Database
            var options = new DbContextOptionsBuilder<QContext>()
            .UseInMemoryDatabase(databaseName: "Qit")
            .Options;

            Logger = Substitute.For<ILogger<QuestionnaireService>>();

            Context = new QContext(options);
            QuestionnaireService = new QuestionnaireService(Context, Logger);

        }

        [Fact]
        public async Task GetQuestionAsync_NewParticipant_CreateANewPartipantAndReturnFirstQuestionAsync()
        {
            //Arrange
            var participantId = Guid.NewGuid();
            var questionVersion = new QuestionnaireVersionHistory
            {
                Id = Guid.NewGuid(),
                Version = 1,
                CreatedOn = DateTime.Now,
            };

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Version = questionVersion,
                QuestionString = "Test Question",
                Order = 0
            };

            await Context.Questions.AddAsync(question);
            await Context.SaveChangesAsync();

            //Act

            var result = await QuestionnaireService.GetQuestionAsync(participantId);

            //Assert

            var participant = await Context.Participants.FirstOrDefaultAsync(x => x.Id == participantId);
            Assert.NotNull(participant);
            Assert.Equal(question.Id, result.Id);
            Assert.Equal(question.QuestionString, result.QuestionString);

        }

        [Fact]
        public async Task GetQuestionAsync_ExistingParticipantStillNotAnswer_ReturnCurrentQuestion()
        {
            //Arrange
            var participantId = Guid.NewGuid();
            var questionVersion = new QuestionnaireVersionHistory
            {
                Id = Guid.NewGuid(),
                Version = 1,
                CreatedOn = DateTime.Now,
            };

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Version = questionVersion,
                QuestionString = "Test Question",
                Order = 0
            };

            var participant = new Participant
            {
                Id = Guid.NewGuid(),
                Answered = false,
                CurrentQuestion = question,
                CreatedOn = DateTime.Now,
                QuestionnaireDone = false,
            };

            await Context.Questions.AddAsync(question);
            await Context.Participants.AddAsync(participant);
            await Context.SaveChangesAsync();

            //Act

            var result = await QuestionnaireService.GetQuestionAsync(participantId);

            //Assert
            Assert.Equal(question.Id, result.Id);
            Assert.Equal(question.QuestionString, result.QuestionString);

        }


        [Fact]
        public async Task GetQuestionAsync_ExistingParticipantAnswered_ReturnNextQuestion()
        {
            //Arrange
            var questionVersion = new QuestionnaireVersionHistory
            {
                Id = Guid.NewGuid(),
                Version = 1,
                CreatedOn = DateTime.Now,
            };

            var q1 = new Question
            {
                Id = Guid.NewGuid(),
                Version = questionVersion,
                QuestionString = "Test Question",
                Order = 0
            };

            var q2 = new Question
            {
                Id = Guid.NewGuid(),
                Version = questionVersion,
                QuestionString = "Test Question 2",
                Order = 1
            };

            var participant = new Participant
            {
                Id = Guid.NewGuid(),
                Answered = true,
                CurrentQuestion = q1,
                CreatedOn = DateTime.Now,
                QuestionnaireDone = false,
            };

            await Context.Questions.AddRangeAsync(new List<Question> { q1, q2 });
            await Context.Participants.AddAsync(participant);
            await Context.SaveChangesAsync();

            //Act

            var result = await QuestionnaireService.GetQuestionAsync(participant.Id);

            //Assert
            Assert.Equal(q2.Id, result.Id);
            Assert.Equal(q2.QuestionString, result.QuestionString);

            var updatedParticipant = await Context.Participants.FirstOrDefaultAsync(x => x.Id == participant.Id);
            Assert.Equal(q2.Id, updatedParticipant.CurrentQuestion.Id);
            Assert.False(updatedParticipant.Answered);

        }

        [Fact]
        public async Task GetQuestionAsync_LastQuestionAnswered_QuestionnaireDoneFlagSetToTrue()
        {
            //Arrange
            var questionVersion = new QuestionnaireVersionHistory
            {
                Id = Guid.NewGuid(),
                Version = 1,
                CreatedOn = DateTime.Now,
            };

            var q1 = new Question
            {
                Id = Guid.NewGuid(),
                Version = questionVersion,
                QuestionString = "Test Question",
                Order = 0
            };

            var q2 = new Question
            {
                Id = Guid.NewGuid(),
                Version = questionVersion,
                QuestionString = "Test Question 2",
                Order = 1
            };

            var participant = new Participant
            {
                Id = Guid.NewGuid(),
                Answered = true,
                CurrentQuestion = q2,
                CreatedOn = DateTime.Now,
                QuestionnaireDone = false,
            };

            await Context.Questions.AddRangeAsync(new List<Question> { q1, q2 });
            await Context.Participants.AddAsync(participant);
            await Context.SaveChangesAsync();

            //Act

            var result = await QuestionnaireService.GetQuestionAsync(participant.Id);

            //Assert
            Assert.Null(result);

            var updatedParticipant = await Context.Participants.FirstOrDefaultAsync(x => x.Id == participant.Id);
            Assert.Equal(q2.Id, updatedParticipant.CurrentQuestion.Id);
            Assert.True(updatedParticipant.QuestionnaireDone);
            Assert.True(updatedParticipant.Answered);

        }

    }
}
