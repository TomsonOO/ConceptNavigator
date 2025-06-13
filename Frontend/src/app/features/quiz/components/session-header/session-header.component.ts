import { Component, Input, Output, EventEmitter } from '@angular/core';
import { QuizSession } from '../../../../shared/models/session.model';

@Component({
  selector: 'app-session-header',
  templateUrl: './session-header.component.html',
  styleUrls: ['./session-header.component.css']
})
export class SessionHeaderComponent {
  @Input() session: QuizSession | null = null;
  @Output() saveSession = new EventEmitter<string>();
  @Output() loadSession = new EventEmitter<string>();
  @Output() generateMore = new EventEmitter<number>();

  showSaveDialog = false;
  sessionName = '';
  moreQuestionsCount = 3;

  onSaveSession() {
    this.saveSession.emit(this.sessionName);
    this.showSaveDialog = false;
    this.sessionName = '';
  }

  onGenerateMore() {
    this.generateMore.emit(this.moreQuestionsCount);
  }

  getProgressPercentage(): number {
    if (!this.session || this.session.totalQuestions === 0) return 0;
    return Math.round((this.session.answeredQuestions / this.session.totalQuestions) * 100);
  }
} 