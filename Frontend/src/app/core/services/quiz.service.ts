import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Quiz, QuizRequest, QuizResponse } from '../../shared/models/quiz.model';
import { 
  ExtendedExplanation, 
  AdaptiveQuizResponse,
  RecordAnswerRequest,
  RecordInterestRequest,
  ExplainMoreRequest,
  GenerateMoreQuestionsRequest
} from '../../shared/models/session.model';

@Injectable({
  providedIn: 'root'
})
export class QuizService {

  constructor(private http: HttpClient) {}

  generateQuiz(request: QuizRequest): Observable<QuizResponse> {
    return this.http.post<QuizResponse>('/api/quiz/generate', request);
  }

  recordAnswer(sessionId: string, questionId: string, answerIndex: number): Observable<void> {
    const request: RecordAnswerRequest = { sessionId, questionId, answerIndex };
    return this.http.post<void>('/api/session/record-answer', request);
  }

  recordInterest(sessionId: string, questionId: string, interestRating: number): Observable<void> {
    const request: RecordInterestRequest = { sessionId, questionId, interestRating };
    return this.http.post<void>('/api/session/record-interest', request);
  }

  getExtendedExplanation(questionId: string, sessionId: string, language: string): Observable<ExtendedExplanation> {
    const request: ExplainMoreRequest = { questionId, sessionId, language };
    return this.http.post<ExtendedExplanation>('/api/quiz/explain-more', request);
  }

  generateMoreQuestions(sessionId: string, count: number): Observable<AdaptiveQuizResponse> {
    const request: GenerateMoreQuestionsRequest = { sessionId, count };
    return this.http.post<AdaptiveQuizResponse>('/api/quiz/generate-more-questions', request);
  }
} 
