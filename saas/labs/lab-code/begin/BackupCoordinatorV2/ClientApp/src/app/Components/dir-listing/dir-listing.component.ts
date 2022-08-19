import { Component, OnInit, Inject } from '@angular/core';
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
export class DirListingComponent implements OnInit {

  public dirListingDTO: DirListingDTO[] = [];
  public genericMsg = {} as GenericMsg2;
  private headers = new Headers({
    'Accept': 'application/json',
    'enctype': 'multipart/form-data'
  });

  public _guid: string | null | undefined;
  _http: HttpClient;
  
  private timerCount = 0;
  private tmp: any = [];
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
    this._http.get<GenericMsg>(environment.appServerURL + '/api/v3/getCache/dirListing-' + this._guid).subscribe(result => {

      this.genericMsg.msg = result.msg;
      this.genericMsg.guid = result.guid;


      var tmp1 = JSON.parse(this.genericMsg.msg);
      tmp1.fileDTOs.forEach((obj: any, index: any) => {

        var fileDTO = {} as DirListingDTO;
        fileDTO.FileName = obj.FileName;
        fileDTO.length = obj.length;
        fileDTO.FileDate = obj.FileDate;
        fileDTO.disabled = false;
        this.dirListingDTO.push(fileDTO);

      });

      console.info(' this.dirListingDTO', this.dirListingDTO);

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
      
      
      this.dirListingDTO.forEach((obj: DirListingDTO, index: any) => {
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
    var timedID:number = this.timerCount;
    this.tmp[timedID] = setInterval(() => {
      this.CheckBackupStatus(pItem.FileName, timedID);
    }, 10 * 1000);
  }
}
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


