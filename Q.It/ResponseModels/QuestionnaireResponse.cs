using System;
using System.Collections.Generic;

namespace Q.It.ResponseModels
{
    public class QuestionnaireResponse
    {
        public Guid? QuestionId { get; set; }
        public string Question { get; set; }
        public string QuestionTopic { get; set; }
        public List<string> Choices { get; set; }
        public bool QuestionnaireEnded { get; set; }
    }
}
