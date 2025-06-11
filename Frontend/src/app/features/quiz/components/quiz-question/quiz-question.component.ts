import { Component, Input } from '@angular/core';
import { QuizQuestion } from '../../../../shared/models/quiz.model';

@Component({
  selector: 'app-quiz-question',
  templateUrl: './quiz-question.component.html',
  styleUrls: ['./quiz-question.component.css']
})
export class QuizQuestionComponent {
  @Input() question!: QuizQuestion;
  @Input() questionNumber!: number;

  selectedAnswer: number | null = null;
  showAnswer = false;

  selectOption(index: number) {
    if (!this.showAnswer) {
      this.selectedAnswer = index;
    }
  }

  revealAnswer() {
    if (this.selectedAnswer !== null) {
      this.showAnswer = true;
    }
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
