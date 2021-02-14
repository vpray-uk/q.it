using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Q.It.Data;
using Q.It.Models;
using Q.It.RequestModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Q.It.Services
{
    public interface IQuestionnaireService
    {

        /// <summary>
        /// Return the first question of the latest version of the questionnaires
        /// </summary>
        /// <returns></returns>
        Task<Question> GetFirstQuestionAsync();
        /// <summary>
        /// Create a new participant
        /// </summary>
        /// <param name="participantId"></param>
        /// <returns></returns>

        Task<Participant> CreateNewParticipantAsync(Guid participantId);
        /// <summary>
        /// Get participant by Id
        /// </summary>
        /// <param name="participantId"></param>
        /// <returns></returns>

        Task<Participant> GetParticipantAsync(Guid participantId);

        /// <summary>
        /// Get next question for participant
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        Task<Question> GetNextQuestionForParticipantAsync(Participant participant);

        /// <summary>
        /// Main method to get a suitable question for the given participant id
        /// </summary>
        /// <param name="participantId"></param>
        /// <returns></returns>
        Task<Question> GetQuestionAsync(Guid participantId);

        /// <summary>
        /// Save answer to database
        /// </summary>
        /// <param name="ans"></param>
        /// <returns></returns>
        Task ProceessAnswerAsync(AnswerRequest ans);

        /// <summary>
        /// Generate a CSV files for participant answers on the fly
        /// </summary>
        /// <param name="participantId"></param>
        /// <returns></returns>
        Task<byte[]> GenerateCSVForAnswersAsync(Guid participantId);

        /// <summary>
        /// Get answers of a given participant ID
        /// </summary>
        /// <param name="participantId"></param>
        /// <returns></returns>
        Task<List<Answer>> GetParticipantAnswersAsync(Guid participantId);

    }
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly QContext Context;
        private readonly ILogger<QuestionnaireService> Logger;
        public QuestionnaireService(QContext context, ILogger<QuestionnaireService> logger)
        {
            Context = context;
            Logger = logger;
        }

        public async Task<Question> GetQuestionAsync(Guid participantId)
        {
            Logger.LogInformation($"Getting a question for participant {participantId} ..");

            var participant = await GetParticipantAsync(participantId);

            if (participant == null)
            {
                Logger.LogInformation($"Cannot find participant {participantId}. Creating a new participant ..");
                await CreateNewParticipantAsync(participantId);
                return await GetFirstQuestionAsync();
            }

            if (participant.QuestionnaireDone)
            {
                Logger.LogInformation($"QuestionnaireDone = true for participant {participantId} ..");
                return null;
            }

            if (participant.Answered)
            {
                Logger.LogInformation($"Answered = true. Getting the next question for participant {participantId} ..");

                var nextQuestion = await GetNextQuestionForParticipantAsync(participant);

                if (nextQuestion == null)
                {
                    Logger.LogInformation($"participant {participantId} has answered all questions.");
                    participant.QuestionnaireDone = true;
                    participant.UpdatedOn = DateTime.Now;
                    Context.Participants.Update(participant);
                    await Context.SaveChangesAsync();
                    return null;
                }

                participant.CurrentQuestion = nextQuestion;
                participant.Answered = false;
                participant.UpdatedOn = DateTime.Now;
                Context.Participants.Update(participant);
                await Context.SaveChangesAsync();
            }

            return participant.CurrentQuestion;
        }


        public async Task<Question> GetNextQuestionForParticipantAsync(Participant participant)
        {
            return await Context.Questions.Include(x => x.Choices).Include(x => x.Version).FirstOrDefaultAsync(x => x.Order == participant.CurrentQuestion.Order + 1 && x.Version.Id == participant.CurrentQuestion.Version.Id);
        }

        public async Task<Question> GetFirstQuestionAsync()
        {
            return await Context.Questions.Include(x => x.Choices).Include(x => x.Version).OrderByDescending(x => x.Version.CreatedOn).FirstOrDefaultAsync(x => x.Order == 0);
        }

        public async Task<Participant> GetParticipantAsync(Guid participantId)
        {
            return await Context.Participants.Include(x => x.CurrentQuestion)
                .Include(x => x.CurrentQuestion.Choices)
                .Include(x => x.CurrentQuestion.Version)
                .Include(x => x.Answers)
                .FirstOrDefaultAsync(x => x.Id == participantId);
        }

        public async Task<Participant> CreateNewParticipantAsync(Guid participantId)
        {
            var newParticipant = new Participant
            {
                Id = participantId,
                CurrentQuestion = await GetFirstQuestionAsync(),
                Answered = false,
                CreatedOn = DateTime.Now,
                QuestionnaireDone = false,
            };

            await Context.Participants.AddAsync(newParticipant);
            await Context.SaveChangesAsync();

            return newParticipant;
        }

        public async Task ProceessAnswerAsync(AnswerRequest ans)
        {

            Logger.LogInformation($"Processing answer for participant {ans.participantId} on question {ans.questionId}");

            var participant = await GetParticipantAsync(ans.participantId);

            if(participant.Answered)
            {
                Logger.LogError($"Participant {ans.participantId} has already answered question {ans.questionId}");
                throw new Exception($"Question already answered.");
            }

            if (participant.QuestionnaireDone)
            {
                Logger.LogError($"Participant {ans.participantId} has already finished the questionnaire.");
                throw new Exception($"Questionnaire already done.");
            }

            if (participant.CurrentQuestion.Id != ans.questionId)
            {
                Logger.LogError($"The answered question ID {ans.questionId} does not match with the participant current question Id {participant.CurrentQuestion.Id}");
                throw new Exception("Question IDs do not match");
            }

            var questionChoices = participant.CurrentQuestion.Choices;

            if(questionChoices != null && questionChoices.Any() &&  !questionChoices.Any(x => x.ChoiceString.Trim().ToLower().Equals(ans.AnswerString.Trim().ToLower())))
            {
                Logger.LogError($"The answer {ans.AnswerString} does not match with the given answer choices of the question {ans.questionId}");
                throw new Exception("Answer choice does not exist.");
            }

            var newAnswer = new Answer
            {
                Id = Guid.NewGuid(),
                AnsweredQuestion = Context.Questions.FirstOrDefault(x => x.Id == ans.questionId),
                AnsweringParticipant = participant,
                AnswerString = ans.AnswerString,
                CreatedOn = DateTime.Now,
            };

            participant.Answered = true;


            // check for answers that forces ending.
            var isFastFinishingAns = Context.FastFinishAnswers.Any(x => x.QuestionId == ans.questionId && x.Answer.Trim().ToLower() == ans.AnswerString.Trim().ToLower());

            if (isFastFinishingAns)
            {
                Logger.LogInformation($"Participant {ans.participantId} gave a fast finishing answer {ans.AnswerString} for the question {ans.questionId}. Ending the questionnaire ..");
                participant.QuestionnaireDone = true;
                participant.UpdatedOn = DateTime.Now;
                
            }

            Context.Participants.Update(participant);
            await Context.Answers.AddAsync(newAnswer);
            await Context.SaveChangesAsync();
        }

        public async Task<List<Answer>> GetParticipantAnswersAsync(Guid participantId)
        {
            return await Context.Answers.Include(x => x.AnsweredQuestion).Where(x => x.AnsweringParticipant.Id == participantId).ToListAsync();
        }

        public async Task<byte[]> GenerateCSVForAnswersAsync(Guid participantId)
        {
            var answers = await GetParticipantAnswersAsync(participantId);

            if(answers == null || !answers.Any())
            {
                Logger.LogWarning($"Cannot find any answer for participant {participantId}");
                return null;
            }

            var selectedAnswers = answers.Select(x => new { Question = x.AnsweredQuestion.QuestionString, Answer = x.AnswerString });

            byte[] csvBytes;

            var stream = new MemoryStream();
            using (var w = new StreamWriter(stream))
            using(var csv = new CsvWriter(w, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(selectedAnswers);
 
            }
            csvBytes = stream.ToArray();
            await stream.DisposeAsync();

            return csvBytes;
        }
    }
}
