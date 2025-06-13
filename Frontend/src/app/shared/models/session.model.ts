import { QuizQuestion } from './quiz.model';

export interface QuizSession {
  id: string;
  sessionName?: string;
  topic: string;
  book?: string;
  questionType: string;
  difficulty: string;
  language: string;
  questions: QuizQuestion[];
  createdAt: string;
  lastModifiedAt: string;
  totalQuestions: number;
  answeredQuestions: number;
  correctAnswers: number;
  isCompleted: boolean;
  averageScore: number;
  averageInterestRating: number;
}

export interface SessionSummary {
  id: string;
  sessionName?: string;
  topic: string;
  book?: string;
  questionType: string;
  difficulty: string;
  language: string;
  createdAt: string;
  lastModifiedAt: string;
  totalQuestions: number;
  answeredQuestions: number;
  correctAnswers: number;
  isCompleted: boolean;
  averageScore: number;
  averageInterestRating: number;
}

export interface ExtendedExplanation {
  questionText: string;
  mainExplanation: string;
  detailedParagraphs: string[];
  historicalContext: string;
  contemporaryRelevance: string;
  keyConcepts: string[];
  relatedPhilosophers: string[];
  suggestedSources: SuggestedSource[];
}

export interface SuggestedSource {
  title: string;
  author: string;
  type: string;
  description?: string;
  relevanceScore: number;
}

export interface AdaptiveQuizResponse {
  sessionId: string;
  questions: QuizQuestion[];
  totalQuestionsInSession: number;
}

export interface RecordAnswerRequest {
  sessionId: string;
  questionId: string;
  answerIndex: number;
}

export interface RecordInterestRequest {
  sessionId: string;
  questionId: string;
  interestRating: number;
}

export interface ExplainMoreRequest {
  questionId: string;
  sessionId: string;
  language: string;
}

export interface GenerateMoreQuestionsRequest {
  sessionId: string;
  count: number;
}

export interface SaveSessionRequest {
  sessionId: string;
  sessionName?: string;
} 