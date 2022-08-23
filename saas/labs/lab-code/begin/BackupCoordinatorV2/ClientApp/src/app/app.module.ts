import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common'
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { DirListingComponent } from './Components/dir-listing/dir-listing.component';
import { LogListComponent } from './Components/log-list/log-list.component';
import { GlobalConstModule } from './Common/global-const/global-const.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { BackupHistoryComponent } from './Components/backup-history/backup-history.component';
import { NgChartsModule } from 'ng2-charts';
@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    DirListingComponent,
    LogListComponent,
    BackupHistoryComponent
    
  ],
  imports: [
   
    CommonModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    GlobalConstModule,
    BrowserAnimationsModule, ToastrModule.forRoot(),
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: ':guid', component: HomeComponent },
      { path: 'counter', component: CounterComponent },
      { path: 'backupHistory/:guid', component: BackupHistoryComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'dirListing/:guid', component: DirListingComponent },
      { path: 'logListing/:guid', component: LogListComponent },
    ]),
    NgChartsModule
  ],
  providers: [],
  
  bootstrap: [AppComponent]
})
export class AppModule { }
