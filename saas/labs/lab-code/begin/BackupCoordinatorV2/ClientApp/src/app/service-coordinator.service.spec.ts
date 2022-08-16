import { TestBed } from '@angular/core/testing';

import { ServiceCoordinatorService } from './service-coordinator.service';

describe('ServiceCoordinatorService', () => {
  let service: ServiceCoordinatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServiceCoordinatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
