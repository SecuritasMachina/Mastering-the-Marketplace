import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ActivatedRoute } from '@angular/router'

@Component({
  selector: 'app-log-list',
  templateUrl: './log-list.component.html',
  styleUrls: ['./log-list.component.css']
})
export class LogListComponent implements OnInit {
  _http: HttpClient;
  public logMsg = {} as LogMsg;
  public _logMsg: LogMsg[] = [];
  public _guid:  string | null | undefined;

  constructor(http: HttpClient, private _Activatedroute: ActivatedRoute) {
    this._http = http;
  }

  ngOnInit(): void {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    this._http.get<LogMsg>(environment.appServerURL + '/api/v3/getLogs/' + this._guid).subscribe(result => {
      console.info('result', result);
      this.logMsg.msg = result.msg;
      
      //this.dirListingDTO = Object.assign({}, tmp1.fileDTOs); 
      //console.info(' this.dirListingDTO', this.dirListingDTO);
      // console.info(' this.dirListingDTO length', this.dirListingDTO.length);
    }, (error: any) => console.error(error));
console.log("this._guid", this._guid);
  }

}
interface LogMsg {
  logTime: string;
  msg: string;
  id: string;
}
