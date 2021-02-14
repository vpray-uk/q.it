using Q.It.Models;
using Q.It.ResponseModels;
using System.Collections.Generic;
using System.Linq;

namespace Q.It.Mapper
{
    public static class Mapper
    {
        public static QuestionnaireResponse ToQuestionnaireResponse(this Question q)
        {
            return new QuestionnaireResponse
            {
                QuestionId = q.Id,
                Question = q.QuestionString,
                Choices = (q.Choices != null && q.Choices.Any()) ? q.Choices.Select(x => x.ChoiceString).ToList() : new List<string>(),
                QuestionnaireEnded = false,
                QuestionTopic = q.QuestionTopic,
            };
        }
    }
}
