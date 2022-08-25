import { Component, OnInit } from '@angular/core';
import { GlobalConstModule } from '../Common/global-const/global-const.module';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router'
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Chart, ChartConfiguration, ChartOptions } from 'chart.js';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {

  public name: string | null | undefined = "Unknown";
  public _guid: string | null | undefined = "Unknown";
  public _AgentConfig!: AgentConfig;
  public _ReportDTO!: ReportDTO;
  public _totalBackups: number = 0;
  public _totalRestores: number = 0;
 
  public _totalOffsiteBackups: number = 0;

  _upTimeChartData: any[] = [
    {
      data: [],
      label: 'CheckIn'
    }
  ];
  _upTimeChartDataLabels: any[] = [];
  _activeBackupsChartData: any[] = [
    {
      data: [],
      label: 'Backups'
    }
  ];
  _activeBackupsDataLabels: any[] = [];

  _offsiteFilesChartData: any[] = [
    {
      data: [0, 0, 0, 0, 0, 0, 0],
      label: 'OffSite Files'
    }
  ];
  _activeRestoresChartData: any[] = [
    {
      data: [0, 0, 0, 0, 0, 0, 0],
      label: 'Active Restores'
    }
  ];


  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July'
    ],
    datasets: [
      {
        data: [0, 0, 0, 0, 0, 0, 0],
        label: 'UpTime',
        fill: true,
        tension: 0.5,
        borderColor: 'black',
        backgroundColor: 'rgba(255,0,0,0.3)'
      }
    ]
  };

  public lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    scales: {
      xAxis: {
        min: 0
      },
      yAxis: {
        min: 0


      }
    }

  };
  public uptimeLineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    scales: {
      xAxis: {
        min: 0
      },
      yAxis: {
        min: 0


      }
    }
  };
  public lineChartLegend = true;

  constructor(private _http: HttpClient, private router: Router, private globalConstModule: GlobalConstModule, private _Activatedroute: ActivatedRoute) {
    this._guid = this._Activatedroute.snapshot.paramMap.get("guid");
    console.warn("this.guid:" + this._guid);
    if (this._guid == null) {
      const queryString = window.location.search;
      const urlParams = new URLSearchParams(queryString);
      GlobalConstModule.guid = urlParams.get('guid');
      console.warn("GlobalConstModule.guid:" + GlobalConstModule.guid);
      this._guid = GlobalConstModule.guid;

    }
  }
  ngOnInit(): void {
    this._http.get<AgentConfig>(environment.appServerURL + '/api/v2/customerConfig/' + this._guid).subscribe(result => {
      this._AgentConfig = result;
     

    }, (error: any) => console.error(error));

    this._http.get<ReportDTO>(environment.appServerURL + '/api/v3/PerfHistory/' + this._guid).subscribe(result => {
      this._ReportDTO = result;
      console.info(' this.ReportDTO', this._ReportDTO);
      
      this._ReportDTO.dirListFileReportItems.forEach((reportItemDTO: ReportItemDTO, index: number): void => {
       
        this._upTimeChartData[0].data.push(reportItemDTO.myCount);
        this._upTimeChartDataLabels.push(reportItemDTO.myDate);
      });
      console.info(" this._upTimeChartDataLabels",this._upTimeChartData, this._upTimeChartDataLabels);
      this._ReportDTO.backupItemsFileReportItems.forEach((reportItemDTO: ReportItemDTO, index: number): void => {
        this._totalBackups += reportItemDTO.myCount;
        this._activeBackupsChartData[0].data.push(reportItemDTO.myCount);
        this._activeBackupsDataLabels.push(reportItemDTO.myDate);
      });
      this._ReportDTO.offSiteFileReportItems.forEach((reportItemDTO: ReportItemDTO, index: number): void => {
    //    this._totalBackups += reportItemDTO.myCount;
      //  this._activeBackupsChartData.push(reportItemDTO.myCount);
       // this._activeBackupsDataLabels.push(reportItemDTO.myDate);
      });
      this._http.get<GenericMsg>(environment.appServerURL + '/api/v3/getCache/dirListing-' + this._guid).subscribe(result => {
       var tmp1 = JSON.parse(result.msg);
      
        tmp1.fileDTOs.forEach((obj: any, index: any) => {
          this._totalOffsiteBackups++;
          
        });
        for (var _chartjsindex in Chart.instances) {
          /* 
           * Here in the config your actual data and options which you have given at the 
             time of creating chart so no need for changing option only you can change data
          */
          //Chart.instances[_chartjsindex].config.data = [];
          // here you can give add your data
          Chart.instances[_chartjsindex].update();
          // update will rewrite your whole chart with new value
        }
       // console.info(' this.dirListingDTO', this._dirListingDTO);

      }, (error: any) => console.error(error));

    }, (error: any) => console.error(error));
    const ctx = new CanvasRenderingContext2D();

   // const chart: Chart = new Chart(ctx, {});
  }
}
interface ReportDTO {
  offSiteFileReportItems: ReportItemDTO[];
  dirListFileReportItems: ReportItemDTO[];
  backupItemsFileReportItems: ReportItemDTO[];
  totalRestores: number;
  lastDateEnteredTimestamp: number;
}
interface ReportItemDTO {
  myCount: number;
  myDate: Date;
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
