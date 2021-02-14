using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Q.It.Mapper;
using Q.It.RequestModels;
using Q.It.ResponseModels;
using Q.It.Services;
using System;
using System.Threading.Tasks;

namespace Q.It.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IQuestionnaireService QuestionnaireService;
        private readonly ILogger<QuestionnaireController> Logger;

        public QuestionnaireController(ILogger<QuestionnaireController> logger, IQuestionnaireService questionnaireService)
        {
            Logger = logger;
            QuestionnaireService = questionnaireService;
        }

        [HttpGet]
        [Route("getQuestion/{participantId}")]
        public async Task<ActionResult> GetQuestionAsync(Guid participantId)
        {
            try
            {
                var question = await QuestionnaireService.GetQuestionAsync(participantId);

                if (question == null)
                {
                    Logger.LogInformation($"Participant {participantId} has already answered all of the questions.");
                    return new JsonResult(new QuestionnaireResponse
                    {
                        QuestionnaireEnded = true
                    });
                }

                return new JsonResult(question.ToQuestionnaireResponse());
            }
            catch(Exception e)
            {
                Logger.LogError($"Exception was found for participant {participantId}. Exception={e}");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new CommonResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    ErrorMessage = $"Failed to get a question. Please contact customer service."
                });
            }
        }

        [HttpPost]
        [Route("answerQuestion")]
        public async Task<ActionResult> PostAnswerAsync(AnswerRequest ans)
        {
            try
            {
                await QuestionnaireService.ProceessAnswerAsync(ans);
                Response.StatusCode = StatusCodes.Status201Created;
                return new JsonResult(new CommonResponse { Status = StatusCodes.Status201Created.ToString() });
            }
            catch(Exception e)
            {
                Logger.LogError($"Exception was found for participant {ans.participantId}. Exception={e}");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new CommonResponse 
                { 
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    ErrorMessage = $"Failed to save the answer. Please contact customer service." 
                });
            }
                 
        }

        [HttpGet]
        [Route("downloadAnswers/{participantId}")]
        public async Task<ActionResult> DownloadAnswersAsync(Guid participantId)
        {
            try
            {
                var streamAsBytes = await QuestionnaireService.GenerateCSVForAnswersAsync(participantId);
                Response.StatusCode = StatusCodes.Status200OK;
                return File(streamAsBytes, "application/octet-stream", $"answers_{participantId}.csv");
            }
            catch(Exception e)
            {
                Logger.LogError($"Failed to generate a csv file for participant {participantId}. Exception = {e}");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new CommonResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    ErrorMessage = $"Failed to generate the csv file. Please contact customer service."
                });
            }
            
        }

    }
}
