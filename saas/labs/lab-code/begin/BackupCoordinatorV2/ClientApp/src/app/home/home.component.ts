import { Component } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router'
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  
  public name: string | null | undefined = "Unknown";
  public _guid: string | null | undefined = "Unknown";

  constructor(private router: Router, private globalConstModule: GlobalConstModule, private _Activatedroute: ActivatedRoute) {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    if (this._guid == null) {
      const queryString = window.location.search;
      const urlParams = new URLSearchParams(queryString);
      GlobalConstModule.guid = urlParams.get('guid');
      console.warn("GlobalConstModule.guid:" + GlobalConstModule.guid);
      this._guid = GlobalConstModule.guid;
      if (GlobalConstModule.guid != null)
        this.router.navigate(['/dirListing/' + GlobalConstModule.guid]);
    }
  }
}
