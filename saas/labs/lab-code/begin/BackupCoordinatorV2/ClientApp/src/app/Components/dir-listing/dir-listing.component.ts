import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
@Component({
  selector: 'app-dir-listing',
  templateUrl: './dir-listing.component.html',
  styleUrls: ['./dir-listing.component.css']
})
export class DirListingComponent implements OnInit {

  public dirListingDTO: DirListingDTO[] = [];
  public genericMsg = {} as GenericMsg2;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    var guid = 'ab50c41e-3814-4533-8f68-a691b4da9043';
    http.get<GenericMsg>(baseUrl + 'v3/getCache/dirListing-' + guid).subscribe(result => {
      console.info('result',result);
      this.genericMsg.msg = result.msg;
      console.info('this.genericMsg.msg',this.genericMsg.msg);
      var tmp1 = JSON.parse(this.genericMsg.msg);
      console.info('tmp1',tmp1);
      this.dirListingDTO = tmp1.fileDTOs;
      console.info(' this.dirListingDTO',this.dirListingDTO);
    }, error => console.error(error));
  }

  ngOnInit(): void {
  }

}
type GenericMsg2 = {
  msgType: string;
  msg: string;
}
interface GenericMsg {
  msgType: string;
  msg: string;
}
interface DirListingDTO {
  FileName: string;
  length: number;
  date: string;
}
