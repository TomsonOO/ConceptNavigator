import { Component, Input, Output, EventEmitter } from '@angular/core';
import { QuizQuestion } from '../../../../shared/models/quiz.model';
import { ExtendedExplanation } from '../../../../shared/models/session.model';

@Component({
  selector: 'app-quiz-question',
  templateUrl: './quiz-question.component.html',
  styleUrls: ['./quiz-question.component.css']
})
export class QuizQuestionComponent {
  @Input() question!: QuizQuestion;
  @Input() questionNumber!: number;
  @Input() sessionId!: string;
  @Output() answerRecorded = new EventEmitter<{questionId: string, answerIndex: number}>();
  @Output() interestRated = new EventEmitter<{questionId: string, rating: number}>();
  @Output() explainMoreRequested = new EventEmitter<string>();

  selectedAnswer: number | null = null;
  showAnswer = false;
  showInterestRating = false;
  showRateButton = false;
  interestRating: number | null = null;
  extendedExplanation: ExtendedExplanation | null = null;
  loadingExplanation = false;

  selectOption(index: number) {
    if (!this.showAnswer) {
      this.selectedAnswer = index;
    }
  }

  revealAnswer() {
    if (this.selectedAnswer !== null) {
      this.showAnswer = true;
      this.showRateButton = true;
      this.answerRecorded.emit({
        questionId: this.question.id,
        answerIndex: this.selectedAnswer
      });
    }
  }

  showRatingOptions() {
    this.showInterestRating = true;
    this.showRateButton = false;
  }

  rateInterest(rating: number) {
    this.interestRating = rating;
    this.showInterestRating = false;
    this.interestRated.emit({
      questionId: this.question.id,
      rating: rating
    });
  }

  requestExtendedExplanation() {
    if (this.loadingExplanation || this.extendedExplanation) return;
    
    this.loadingExplanation = true;
    this.explainMoreRequested.emit(this.question.id);
  }

  setExtendedExplanation(explanation: ExtendedExplanation) {
    this.extendedExplanation = explanation;
    this.loadingExplanation = false;
  }

  getOptionLabel(index: number): string {
    return String.fromCharCode(65 + index);
  }

  isCorrectAnswer(index: number): boolean {
    return index === this.question.correctAnswerIndex;
  }

  isSelectedAnswer(index: number): boolean {
    return this.selectedAnswer === index;
  }
} 
