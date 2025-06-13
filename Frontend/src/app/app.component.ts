import { Component, ViewChild, QueryList, ViewChildren } from '@angular/core';
import { QuizService } from './core/services/quiz.service';
import { SessionService } from './core/services/session.service';
import { Quiz, QuizRequest, QuizResponse } from './shared/models/quiz.model';
import { QuizSession, ExtendedExplanation } from './shared/models/session.model';
import { QuizFormComponent } from './features/quiz/components/quiz-form/quiz-form.component';
import { QuizQuestionComponent } from './features/quiz/components/quiz-question/quiz-question.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  @ViewChild(QuizFormComponent) quizForm!: QuizFormComponent;
  @ViewChildren(QuizQuestionComponent) questionComponents!: QueryList<QuizQuestionComponent>;
  
  quiz: Quiz | null = null;
  currentSession: QuizSession | null = null;
  sessionId: string | null = null;
  extendedExplanations: Map<string, ExtendedExplanation> = new Map();

  constructor(
    private quizService: QuizService,
    private sessionService: SessionService
  ) {}

  onGenerateQuiz(request: QuizRequest) {
    this.quizForm.setLoading(true);
    this.quiz = null;
    this.currentSession = null;
    this.extendedExplanations.clear();

    this.quizService.generateQuiz(request)
      .subscribe({
        next: (response: QuizResponse) => {
          this.sessionId = response.sessionId;
          this.quiz = {
            topic: response.topic,
            questions: response.questions
          };
          this.currentSession = this.convertResponseToSession(response);
          this.quizForm.setLoading(false);
        },
        error: (error) => {
          console.error('Error generating quiz:', error);
          this.quizForm.setLoading(false);
        }
      });
  }

  onAnswerRecorded(event: {questionId: string, answerIndex: number}) {
    if (!this.sessionId) return;
    
    const question = this.quiz?.questions.find(q => q.id === event.questionId);
    if (question) {
      question.userAnswerIndex = event.answerIndex;
      question.isCorrect = event.answerIndex === question.correctAnswerIndex;
    }
    
    this.quizService.recordAnswer(this.sessionId, event.questionId, event.answerIndex)
      .subscribe({
        next: () => {
          this.updateQuestionInSession(event.questionId, { 
            userAnswerIndex: event.answerIndex,
            isCorrect: event.answerIndex === question?.correctAnswerIndex
          });
        },
        error: (error) => console.error('Error recording answer:', error)
      });
  }

  onInterestRated(event: {questionId: string, rating: number}) {
    if (!this.sessionId) return;
    
    const question = this.quiz?.questions.find(q => q.id === event.questionId);
    if (question) {
      question.userInterestRating = event.rating;
    }
    
    this.quizService.recordInterest(this.sessionId, event.questionId, event.rating)
      .subscribe({
        next: () => {
          this.updateQuestionInSession(event.questionId, { userInterestRating: event.rating });
        },
        error: (error) => console.error('Error recording interest:', error)
      });
  }

  onExplainMoreRequested(questionId: string) {
    if (!this.sessionId) return;
    
    this.quizService.getExtendedExplanation(questionId, this.sessionId, 'English')
      .subscribe({
        next: (explanation: ExtendedExplanation) => {
          this.extendedExplanations.set(questionId, explanation);
          const questionComponent = this.questionComponents.find(comp => comp.question.id === questionId);
          if (questionComponent) {
            questionComponent.setExtendedExplanation(explanation);
          }
        },
        error: (error) => {
          console.error('Error getting extended explanation:', error);
          const questionComponent = this.questionComponents.find(comp => comp.question.id === questionId);
          if (questionComponent) {
            questionComponent.loadingExplanation = false;
          }
        }
      });
  }

  onGenerateMoreQuestions(count: number) {
    if (!this.sessionId) return;
    
    this.quizService.generateMoreQuestions(this.sessionId, count)
      .subscribe({
        next: (response) => {
          if (this.quiz && this.currentSession) {
            this.quiz.questions = [...this.quiz.questions, ...response.questions];
            this.currentSession.questions = [...this.currentSession.questions, ...response.questions];
            this.currentSession.totalQuestions = response.totalQuestionsInSession;
            this.updateSessionStats();
          }
        },
        error: (error) => console.error('Error generating more questions:', error)
      });
  }

  onSaveSession(sessionName: string) {
    if (!this.sessionId) return;
    
    this.sessionService.saveSession(this.sessionId, sessionName)
      .subscribe({
        next: () => {
          console.log('Session saved successfully');
          if (this.currentSession) {
            this.currentSession.sessionName = sessionName;
          }
        },
        error: (error) => console.error('Error saving session:', error)
      });
  }

  getExtendedExplanation(questionId: string): ExtendedExplanation | null {
    return this.extendedExplanations.get(questionId) || null;
  }

  private convertResponseToSession(response: QuizResponse): QuizSession {
    return {
      id: response.sessionId,
      topic: response.topic,
      book: response.book,
      questionType: response.questionType,
      difficulty: response.difficulty,
      language: response.language,
      questions: response.questions,
      createdAt: response.createdAt,
      lastModifiedAt: response.createdAt,
      totalQuestions: response.questions.length,
      answeredQuestions: 0,
      correctAnswers: 0,
      isCompleted: false,
      averageScore: 0,
      averageInterestRating: 0
    };
  }

  private updateQuestionInSession(questionId: string, updates: any) {
    if (!this.currentSession) return;
    
    const question = this.currentSession.questions.find(q => q.id === questionId);
    if (question) {
      Object.assign(question, updates);
      this.updateSessionStats();
    }
  }

  private updateSessionStats() {
    if (!this.currentSession) return;
    
    const answeredQuestions = this.currentSession.questions.filter(q => q.userAnswerIndex !== undefined);
    const correctAnswers = answeredQuestions.filter(q => q.isCorrect === true);
    const ratedQuestions = this.currentSession.questions.filter(q => q.userInterestRating !== undefined);
    
    this.currentSession.answeredQuestions = answeredQuestions.length;
    this.currentSession.correctAnswers = correctAnswers.length;
    this.currentSession.averageScore = answeredQuestions.length > 0 ? correctAnswers.length / answeredQuestions.length : 0;
    this.currentSession.averageInterestRating = ratedQuestions.length > 0 
      ? ratedQuestions.reduce((sum, q) => sum + (q.userInterestRating || 0), 0) / ratedQuestions.length 
      : 0;
    this.currentSession.isCompleted = this.currentSession.answeredQuestions === this.currentSession.totalQuestions;
  }

  getOptionLabel(index: number): string {
    return String.fromCharCode(65 + index);
  }
} 
