import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PinLoginComponent } from './pin-login.component';

describe('PinLoginComponent', () => {
  let component: PinLoginComponent;
  let fixture: ComponentFixture<PinLoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PinLoginComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PinLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
