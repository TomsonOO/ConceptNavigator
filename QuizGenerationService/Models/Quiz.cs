namespace QuizGenerationService.Models;

public class Quiz {
  public string Topic { get; set; } = string.Empty;
  public List<QuizQuestion> Questions { get; set; } = new();
}

public class QuizQuestion
{
  public string Question { get; set; } = string.Empty;
  public List<string> Options { get; set; } = new();
  public int CorrectAnswerIndex { get; set; }
}

