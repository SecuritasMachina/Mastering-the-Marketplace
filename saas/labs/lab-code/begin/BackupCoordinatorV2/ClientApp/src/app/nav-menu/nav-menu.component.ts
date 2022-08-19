import { Component } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  public static guid?: string | null | undefined;

  collapse() {
    this.isExpanded = false;
  }
  constructor(private router: Router, private globalConstModule: GlobalConstModule) {
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);
    GlobalConstModule.guid = urlParams.get('guid')
    if (GlobalConstModule.guid != null)
      this.router.navigate(['/dirListing/' + GlobalConstModule.guid]);
  }
  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
