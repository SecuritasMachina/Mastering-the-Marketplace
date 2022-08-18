import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ActivatedRoute } from '@angular/router'
import { NavMenuComponent } from '../../nav-menu/nav-menu.component';
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
  
  id: string | null | undefined;
  _http: HttpClient;
  // const options = new RequestOptions({ headers: this.headers });

  constructor(http: HttpClient, private _Activatedroute: ActivatedRoute) {
    //var guid = 'ab50c41e-3814-4533-8f68-a691b4da9043';
    this._http = http;
    console.info('environment.appServerURL', environment.appServerURL);
    
  }

  ngOnInit(): void {
    this.id = this._Activatedroute.snapshot.paramMap.get("id");
    this._http.get<GenericMsg>(environment.appServerURL + '/api/v3/getCache/dirListing-' + this.id).subscribe(result => {
      console.info('result', result);
      this.genericMsg.msg = result.msg;
      this.genericMsg.guid = result.guid;
      //this.guid = result.guid;

      var tmp1 = JSON.parse(this.genericMsg.msg);
      tmp1.fileDTOs.forEach((obj: any, index: any) => {
        // console.log('before transform, this : ' + this);
        var fileDTO = {} as DirListingDTO;
        fileDTO.FileName = obj.FileName;
        fileDTO.length = obj.length;
        this.dirListingDTO.push(fileDTO);

      });
      //this.dirListingDTO = Object.assign({}, tmp1.fileDTOs); 
      console.info(' this.dirListingDTO', this.dirListingDTO);
      // console.info(' this.dirListingDTO length', this.dirListingDTO.length);
    }, (error: any) => console.error(error));
    console.log("this.id",this.id);
  }
  async restoreFile(pRestoreFileName: string): Promise<void> {
    console.info(' pRestoreFileName', pRestoreFileName);
    //this.httpCall("POST", pRestoreFileName)
    const response = await fetch(environment.appServerURL + "/api/v2/requestRestore", {
      method: 'POST',
      body: JSON.stringify({
        backupName: pRestoreFileName,
        customerGUID: NavMenuComponent.guid,
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
}
