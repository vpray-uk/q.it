using System;

namespace Q.It.RequestModels
{
    public class AnswerRequest
    {
        public Guid participantId {get;set;}
        public Guid questionId { get; set; }
        public string AnswerString { get; set; }
    }
}
