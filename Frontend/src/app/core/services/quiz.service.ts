import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Quiz, QuizRequest } from '../../shared/models/quiz.model';

@Injectable({
  providedIn: 'root'
})
export class QuizService {

  constructor(private http: HttpClient) {}

  generateQuiz(request: QuizRequest): Observable<Quiz> {
    return this.http.post<Quiz>('/api/quiz/generate', request);
  }
} 
