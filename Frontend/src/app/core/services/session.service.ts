import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  QuizSession, 
  SessionSummary, 
  SaveSessionRequest 
} from '../../shared/models/session.model';

@Injectable({
  providedIn: 'root'
})
export class SessionService {

  constructor(private http: HttpClient) {}

  getSession(sessionId: string): Observable<QuizSession> {
    return this.http.get<QuizSession>(`/api/session/${sessionId}`);
  }

  saveSession(sessionId: string, sessionName?: string): Observable<string> {
    const request: SaveSessionRequest = { sessionId, sessionName };
    return this.http.post<string>('/api/session', request);
  }

  getSessionSummaries(): Observable<SessionSummary[]> {
    return this.http.get<SessionSummary[]>('/api/session');
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`/api/session/${sessionId}`);
  }
} 