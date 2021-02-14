# Q.It - Simple Questionnaire API
In short, this project mainly provides API endpoints for doing questionnaires. 

# Prerequisites

1. .NET SDK 5 
   > Link: https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-5.0.103-windows-x64-installer 
2. SQL Server with default instance (.) and trusted connection enabled. They are enabled by default. Otherwise, adjust the connection string in the appsettings.json accordingly.
3. EF Core CLI tool ```dotnet tool install --global dotnet-ef```
4. Trusted dotnet dev cert for SSL enabled ```dotnet dev-certs https --trust```

# Setup
1. Launch Powershell and cd to root/vts
2. execute ```dotnet ef database update```. The database **Qit** with **seed data** will be created. 
3. execute ```dotnet run``` and the API server with Kestrel will start. The base urls are ```https://localhost:5001``` and ```http://localhost:5000```

# Design
Q.It consists of three endpoints ..
### Endpoint: /Questionnaire/getQuestion/{participantId}
This endpoint has to be called first in order for a participant to get a question. If a given participant ID does not exist in the database, **a new Participant will be created** and **the first question of the latest version of questionnaires will be returned**.

If an existing participant calls this endpoint after the current question has been answered, the next question of the same questionnaire version will be returned or nothing if that question is the last question of the questionnaire.

The returned question can be a question with choices or an open question, depending on the choices field of the returned JSON. If choices field is empty then the question is an open question.

### Endpoint: /Questionnaire/answerQuestion
This endpoint needs to be called when a participant wants to answer a question -- commonly after *getQuestion* has been called. When answerQuestion is called, the answered flag of the participant is set to true. When *getQuestion* is called again, the next question is returned.

It has to be noted that there are some answers of questions that will lead to an end of the questionnaire. Those questions and answers are specified in the FastFinishAnswer table.

### Endpoint: /Questionnaire/downloadAnswers/{participantId}
This endpoints return all of the question-answer pairs in a csv file for a given participant.

## Tests
The solution comes with a modest number of unit test cases to ensure main functionalities work. xUnit framework was used for the tests.


