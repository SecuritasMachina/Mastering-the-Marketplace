import { Component } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router'
@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  public _guid: string | null;

  collapse() {
    this.isExpanded = false;
  }
  constructor(private router: Router, private globalConstModule: GlobalConstModule, private _Activatedroute: ActivatedRoute) {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    console.info("this._guid", this._guid);

    if (this._guid == null) {
      const queryString = window.location.search;
      const urlParams = new URLSearchParams(queryString);
      GlobalConstModule.guid = urlParams.get('guid')
      if (GlobalConstModule.guid != null) {
        this.router.navigate(['/dirListing/' + GlobalConstModule.guid]);
        this._guid = GlobalConstModule.guid;
      }
      else {
        GlobalConstModule.guid = this._guid;
      }
    } else {
      GlobalConstModule.guid = this._guid;
    }
  }
  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
