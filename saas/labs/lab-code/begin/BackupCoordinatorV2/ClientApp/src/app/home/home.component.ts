import { Component, OnInit } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router'
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {

  public name: string | null | undefined = "Unknown";
  public _guid: string | null | undefined = "Unknown";
  public _AgentConfig!: AgentConfig;

  constructor(private _http: HttpClient,private router: Router, private globalConstModule: GlobalConstModule, private _Activatedroute: ActivatedRoute) {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    console.warn("this.guid:" + this._guid);
    if (this._guid == null) {
      const queryString = window.location.search;
      const urlParams = new URLSearchParams(queryString);
      GlobalConstModule.guid = urlParams.get('guid');
      console.warn("GlobalConstModule.guid:" + GlobalConstModule.guid);
      this._guid = GlobalConstModule.guid;
      if (GlobalConstModule.guid != null)
        this.router.navigate(['/' + GlobalConstModule.guid]);
    }
  }
  ngOnInit(): void {
    this._http.get<AgentConfig>(environment.appServerURL + '/api/v2/customerConfig/' + this._guid).subscribe(result => {
      this._AgentConfig = result;
      console.info(' this.backupListingDTO', this._AgentConfig);

    }, (error: any) => console.error(error));
  }
}
interface AgentConfig {
  ServiceBusEndPoint: string;
  ServiceBusSubscription: string;
  topicName: string;
  passPhrase: string;
  subscriptionActive: boolean;
  name: string;
  contactEmail: string;
}
