namespace QuizGenerationService.Queries;

public class GenerateQuizQuery
{
  public string Topic { get; set; } = string.Empty;
  public string Difficulty { get; set; } = "medium";
  public int QuestionCount { get; set; } = 5;
  public string Language { get; set; } = "polish";
}

