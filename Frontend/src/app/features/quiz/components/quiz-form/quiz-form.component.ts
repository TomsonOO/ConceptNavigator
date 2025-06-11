import { Component, EventEmitter, Output } from '@angular/core';
import { QuizRequest } from '../../../../shared/models/quiz.model';

@Component({
  selector: 'app-quiz-form',
  templateUrl: './quiz-form.component.html',
  styleUrls: ['./quiz-form.component.css']
})
export class QuizFormComponent {
  @Output() generateQuiz = new EventEmitter<QuizRequest>();

  topic = '';
  difficulty = 'medium';
  questionCount = 5;
  language = 'polish';
  loading = false;

  onGenerateQuiz() {
    if (!this.topic.trim()) return;
    
    const request: QuizRequest = {
      topic: this.topic,
      difficulty: this.difficulty,
      questionCount: this.questionCount,
      language: this.language
    };

    this.generateQuiz.emit(request);
  }

  setLoading(loading: boolean) {
    this.loading = loading;
  }
} 
