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
  private intervalID: any;
    public restoreInProgress: boolean | undefined;
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
        this.dirListingDTO.push(fileDTO);

      });

      console.info(' this.dirListingDTO', this.dirListingDTO);

    }, (error: any) => console.error(error));

  }
  async CheckBackupStatus(pRestoreFileName: string) {
    var pGuid = this._guid;
    //throw new Error('Function not implemented.');
    console.info(' pRestoreFileName', pRestoreFileName);
    //this.httpCall("POST", pRestoreFileName)
    this._http.get<GenericMsg>(environment.appServerURL + "/api/v3/getCache/" + pRestoreFileName + "-BACKUPFINISHED-" + pGuid).subscribe(result => {
      console.log("response:", result);
      //console.log("response.msg:", result.msg);
      clearTimeout(this.intervalID);
      this.toastr.success(pRestoreFileName + ' restored','Restore Status');
      this.restoreInProgress = false;
    });

    
  }
  async restoreFile(pRestoreFileName: string): Promise<void> {
    console.info(' pRestoreFileName', pRestoreFileName);
    this.restoreInProgress = true;
    //this.httpCall("POST", pRestoreFileName)
    const response = await fetch(environment.appServerURL + "/api/v2/requestRestore/" + this._guid, {
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
    this.toastr.success(pRestoreFileName + ' submitted for restored', 'Restore Status');
    if (!response.ok) {
      throw new Error(`Error! status: ${response.status}`);
    }
    let guid = this._guid;
     this.intervalID = setInterval(() => {
      this.CheckBackupStatus(pRestoreFileName);
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
}


