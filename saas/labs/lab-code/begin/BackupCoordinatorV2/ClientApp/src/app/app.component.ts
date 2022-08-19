import { Component } from '@angular/core';
import { NotificationService } from './Services/notification.service';

//import { NotificationService } from '.';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
  constructor(private notifyService: NotificationService) { }

  showToasterSuccess() {
    this.notifyService.showSuccess(
      'Data shown successfully !!',
      'codingshiksha.com'
    );
  }

  showToasterError() {
    this.notifyService.showError('Something is wrong', 'codingshiksha.com');
  }

  showToasterInfo() {
    this.notifyService.showInfo('This is info', 'codingshiksha.com');
  }

  showToasterWarning() {
    this.notifyService.showWarning('This is warning', 'codingshiksha.com');
  }
}
