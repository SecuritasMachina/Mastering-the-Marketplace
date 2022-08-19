import { Component } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  
  public name: string | null | undefined = "Unknown";
  public guid: string | null | undefined = "Unknown";

  constructor(private router: Router) {
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);
    GlobalConstModule.guid = urlParams.get('guid');
    console.warn("GlobalConstModule.guid:" + GlobalConstModule.guid);
    this.guid = GlobalConstModule.guid;
    if (GlobalConstModule.guid != null)
      this.router.navigate(['/dirListing/' + GlobalConstModule.guid]);
  }
}
