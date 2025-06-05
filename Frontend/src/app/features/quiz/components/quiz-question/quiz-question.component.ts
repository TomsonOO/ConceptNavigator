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

  getOptionLabel(index: number): string {
    return String.fromCharCode(65 + index);
  }
} 
