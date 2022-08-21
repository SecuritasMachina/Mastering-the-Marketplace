import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ActivatedRoute } from '@angular/router'
import { GlobalConstModule } from '../../Common/global-const/global-const.module';
import { NotificationService } from '../../Services/notification.service';
//import { NavMenuComponent } from '../../nav-menu/nav-menu.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dir-listing',
  templateUrl: './dir-listing.component.html',
  styleUrls: ['./dir-listing.component.css']
})



export class DirListingComponent implements OnInit, OnDestroy {


  public genericMsg = {} as GenericMsg2;
  private headers = new Headers({
    'Accept': 'application/json',
    'enctype': 'multipart/form-data'
  });

  public _guid: string | null | undefined;
  _http: HttpClient;
  public _dirListingDTO: DirListingDTO[] = [];
  private timerCount = 0;
  private tmp: any = [];
  private timerId: any;
  //public restoreInProgress: boolean | undefined;
  // const options = new RequestOptions({ headers: this.headers });

  constructor(http: HttpClient, private _Activatedroute: ActivatedRoute, private toastr: ToastrService) {

    this._http = http;
    console.info('environment.appServerURL', environment.appServerURL);

  }

  ngOnInit(): void {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    console.info("DirListingComponent GlobalConstModule.guid1", GlobalConstModule.guid);
    console.log("this._guid", this._guid);
    this.syncDirList();
    this.timerId = setInterval(() => {
      this.syncDirList();
    }, 60 * 1000);
    //new Timer(this._http, dirListingDTO, this._guid, this.syncDirList()).doTimer();


  }
  ngOnDestroy(): void {
    clearInterval(this.timerId);
  }
  syncDirList(): void {
    this._http.get<GenericMsg>(environment.appServerURL + '/api/v3/getCache/dirListing-' + this._guid).subscribe(result => {

      //this.genericMsg.msg = result.msg;
      //  this.genericMsg.guid = result.guid;


      var tmp1 = JSON.parse(result.msg);
      this._dirListingDTO = [];
      tmp1.fileDTOs.forEach((obj: any, index: any) => {

        var fileDTO = {} as DirListingDTO;
        fileDTO.FileName = obj.FileName;
        fileDTO.length = obj.length;
        fileDTO.FileDate = obj.FileDate;
        fileDTO.disabled = false;
        this._dirListingDTO.push(fileDTO);

      });

      console.info(' this.dirListingDTO', this._dirListingDTO);

    }, (error: any) => console.error(error));

  }
  async CheckBackupStatus(pRestoreFileName: string, pTimerCount: number) {
    var pGuid = this._guid;
    //throw new Error('Function not implemented.');
    console.info(' pRestoreFileName', pRestoreFileName);
    //this.httpCall("POST", pRestoreFileName)
    this._http.get<GenericMsg>(environment.appServerURL + "/api/v3/getCache/" + encodeURIComponent(pRestoreFileName + "-restoreComplete-" + pGuid)).subscribe(result => {
      console.log("getCache response:", result);
      //result.
      //console.log("getCache this.tmp[pTimerCount]:", this.tmp[pTimerCount], pTimerCount);
      clearInterval(this.tmp[pTimerCount]);
      //clearTimeout(this.tmp[pTimerCount]);
      //this.tmp[pTimerCount] = null;


      dirListingDTO.forEach((obj: DirListingDTO, index: any) => {
        if (pRestoreFileName == obj.FileName || encodeURI(pRestoreFileName) == encodeURIComponent(obj.FileName)) {
          obj.disabled = false;
        }
      });
      this.toastr.success(pRestoreFileName + ' restored', 'Restore Status');
      this._http.get<string>(environment.appServerURL + "/api/v3/deleteCache/" + encodeURIComponent(pRestoreFileName + "-restoreComplete-" + pGuid)).subscribe(result => {
        console.log("deleteCache response:", result);
      });
      return false;
    }, error => {
      if (error.status != 404)
        console.log("error response:", error);
    });

    return true;
  }
  async restoreFile(pItem: DirListingDTO): Promise<void> {
    clearInterval(this.timerId);
    console.info('restoreFile.pRestoreFileName', pItem.FileName);

    //pItem.disabled = true;

    const response = await fetch(environment.appServerURL + "/api/v2/requestRestore/" + this._guid, {
      method: 'POST',
      body: JSON.stringify({
        backupName: pItem.FileName,
        customerGUID: this._guid,
      }),
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
    });
    this.toastr.success(pItem.FileName + ' submitted for restoration', 'Restore Status');
    if (!response.ok) {
      throw new Error(`Error! status: ${response.status}`);
    }
    this.timerCount++;
    var timedID: number = this.timerCount;
    this.tmp[timedID] = setInterval(() => {
      this.CheckBackupStatus(pItem.FileName, timedID);
    }, 10 * 1000);
  }
}
var dirListingDTO: DirListingDTO[] = [];
type GenericMsg2 = {
  msgType: string;
  msg: string;
  guid: string;
}
interface GenericMsg {
  msgType: string;
  msg: string;
  guid: string;
}
interface DirListingDTO {
  FileName: string;
  length: number;
  date: string;
  FileDate: number;
  disabled: boolean;
}
function delay(delay: number) {
  return new Promise(r => {
    setTimeout(r, delay);
  })
}

class Timer {
  constructor(private _http: HttpClient, public dirListingDTO: DirListingDTO[], private _guid: string | null, private callback?: Function) {
    this.doTimer();
  }
  async doTimer() {
    while (true) {
      await delay(10 * 1000);

      this._http.get<GenericMsg>(environment.appServerURL + '/api/v3/getCache/dirListing-' + this._guid).subscribe(result => {

        //this.genericMsg.msg = result.msg;
        //  this.genericMsg.guid = result.guid;


        var tmp1 = JSON.parse(result.msg);
        this.dirListingDTO = [];
        tmp1.fileDTOs.forEach((obj: any, index: any) => {

          var fileDTO = {} as DirListingDTO;
          fileDTO.FileName = obj.FileName;
          fileDTO.length = obj.length;
          fileDTO.FileDate = obj.FileDate;
          fileDTO.disabled = false;
          this.dirListingDTO.push(fileDTO);

        });
        dirListingDTO = this.dirListingDTO;
        this.callback?.arguments
        this.callback?.call(delay);

        //console.info(' this.dirListingDTO', this.dirListingDTO);

      }, (error: any) => console.error(error));

      //  console.log(this.counter);
    }
  }
}
