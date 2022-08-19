import { Component } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
//import { NavMenuComponent } from '../nav-menu/nav-menu.component';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public guid: string | null | undefined;
  public name: string | null | undefined;

  constructor(private router: Router) {
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);
    GlobalConstModule.guid = urlParams.get('guid');
    console.warn("NavMenuComponent.guid]:" + GlobalConstModule.guid);
    if (GlobalConstModule.guid != null)
      this.router.navigate(['/dirListing/' + GlobalConstModule.guid]);
  }
}
