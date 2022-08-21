import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ActivatedRoute } from '@angular/router'

@Component({
  selector: 'app-log-list',
  templateUrl: './log-list.component.html',
  styleUrls: ['./log-list.component.css']
})
export class LogListComponent implements OnInit, OnDestroy {
  _http: HttpClient;
  public logMsg = {} as LogMsg;
  public _logMsg: LogMsg[] = [];
  public _guid: string | null | undefined;
    timerId: any;

  constructor(http: HttpClient, private _Activatedroute: ActivatedRoute) {
    this._http = http;
  }

  ngOnInit(): void {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    this.syncDirList();
    this.timerId = setInterval(() => {
      this.syncDirList();
    }, 50 * 1000);


    console.log("this._guid", this._guid);
  }
  ngOnDestroy(): void {
    clearInterval(this.timerId);
  }
  syncDirList(): void {
    this._http.get<LogMsg[]>(environment.appServerURL + '/api/v3/getLogs/' + this._guid).subscribe(result => {
      console.info('result', result);
      this._logMsg = result;

      console.info('this.logMsg.msg', this._logMsg);

      //this.dirListingDTO = Object.assign({}, tmp1.fileDTOs); 
      //console.info(' this.dirListingDTO', this.dirListingDTO);
      // console.info(' this.dirListingDTO length', this.dirListingDTO.length);
    }, (error: any) => console.error(error));


  }
  async view(pRestoreFileName: string): Promise<void> {
    console.info(' pRestoreFileName', pRestoreFileName);
    //this.httpCall("POST", pRestoreFileName)
    const response = await fetch(environment.appServerURL + "/api/v2/requestRestore", {
      method: 'POST',
      body: JSON.stringify({
        backupName: pRestoreFileName,
        customerGUID: this._guid,
      }),
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Error! status: ${response.status}`);
    }
  }
}

interface LogMsg {
  logTime: string;
  msg: string;
  id: string;
}
