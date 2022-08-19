import { Injectable, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';



@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ]
})

export class GlobalConstModule {
  //@Injectable({ providedIn: 'app' })
  public static guid: string | null | undefined;
}
