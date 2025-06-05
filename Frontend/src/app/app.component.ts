import { Component, ViewChild } from '@angular/core';
import { QuizService } from './core/services/quiz.service';
import { Quiz, QuizRequest } from './shared/models/quiz.model';
import { QuizFormComponent } from './features/quiz/components/quiz-form/quiz-form.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  @ViewChild(QuizFormComponent) quizForm!: QuizFormComponent;
  
  quiz: Quiz | null = null;

  constructor(private quizService: QuizService) {}

  onGenerateQuiz(request: QuizRequest) {
    this.quizForm.setLoading(true);
    this.quiz = null;

    this.quizService.generateQuiz(request)
      .subscribe({
        next: (result) => {
          this.quiz = result;
          this.quizForm.setLoading(false);
        },
        error: (error) => {
          console.error('Error generating quiz:', error);
          this.quizForm.setLoading(false);
        }
      });
  }

  getOptionLabel(index: number): string {
    return String.fromCharCode(65 + index);
  }
} 
