import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { QuizFormComponent } from './components/quiz-form/quiz-form.component';
import { QuizDisplayComponent } from './components/quiz-display/quiz-display.component';
import { QuizQuestionComponent } from './components/quiz-question/quiz-question.component';
import { SessionHeaderComponent } from './components/session-header/session-header.component';

@NgModule({
  declarations: [
    QuizFormComponent,
    QuizDisplayComponent,
    QuizQuestionComponent,
    SessionHeaderComponent
  ],
  imports: [
    SharedModule
  ],
  exports: [
    QuizFormComponent,
    QuizDisplayComponent,
    QuizQuestionComponent,
    SessionHeaderComponent
  ]
})
export class QuizModule { } 
