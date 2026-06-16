import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusupdateModalComponent } from './statusupdate-modal.component';

describe('StatusupdateModalComponent', () => {
  let component: StatusupdateModalComponent;
  let fixture: ComponentFixture<StatusupdateModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusupdateModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StatusupdateModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
