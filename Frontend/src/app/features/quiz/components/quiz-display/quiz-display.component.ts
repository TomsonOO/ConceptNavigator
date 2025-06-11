import { Component, Input } from '@angular/core';
import { Quiz } from '../../../../shared/models/quiz.model';

@Component({
  selector: 'app-quiz-display',
  templateUrl: './quiz-display.component.html',
  styleUrls: ['./quiz-display.component.css']
})
export class QuizDisplayComponent {
  @Input() quiz!: Quiz;
} 
