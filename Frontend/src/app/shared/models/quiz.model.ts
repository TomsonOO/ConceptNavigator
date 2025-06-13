export interface QuizRequest {
  topic: string;
  book?: string;
  questionType: string;
  difficulty: string;
  questionCount: number;
  language: string;
}

export interface QuizResponse {
  sessionId: string;
  topic: string;
  book?: string;
  questionType: string;
  difficulty: string;
  language: string;
  questions: QuizQuestion[];
  createdAt: string;
}

export interface QuizQuestion {
  id: string;
  question: string;
  options: string[];
  correctAnswerIndex: number;
  explanation: string;
  keywords: string[];
  userAnswerIndex?: number;
  isCorrect?: boolean;
  userInterestRating?: number;
  createdAt: string;
  answeredAt?: string;
}

export interface Quiz {
  topic: string;
  questions: QuizQuestion[];
} 