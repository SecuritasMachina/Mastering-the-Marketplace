import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DirListingComponent } from './dir-listing.component';

describe('DirListingComponent', () => {
  let component: DirListingComponent;
  let fixture: ComponentFixture<DirListingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DirListingComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DirListingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
