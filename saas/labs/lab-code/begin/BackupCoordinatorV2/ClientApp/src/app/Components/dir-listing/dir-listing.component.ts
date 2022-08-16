import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
@Component({
  selector: 'app-dir-listing',
  templateUrl: './dir-listing.component.html',
  styleUrls: ['./dir-listing.component.css']
})
export class DirListingComponent implements OnInit {

  public dirListingDTO: DirListingDTO[] = [];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    var guid = 'ab50c41e-3814-4533-8f68-a691b4da9043';
    http.get<DirListingDTO[]>(baseUrl+'v3/getCache/' + 'dirlisting-' + guid).subscribe(result => {
      this.dirListingDTO = result;
    }, error => console.error(error));
  }

  ngOnInit(): void {
  }

}
interface DirListingDTO {
  fileName: string;
  length: number;
  date: string;
}
