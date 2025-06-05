export interface QuizRequest {
  topic: string;
  difficulty: string;
  questionCount: number;
}

export interface QuizQuestion {
  question: string;
  options: string[];
  correctAnswerIndex: number;
}

export interface Quiz {
  topic: string;
  questions: QuizQuestion[];
} 