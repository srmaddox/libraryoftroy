import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OverlayCheckoutConfirmComponent } from './overlay-checkout-confirm.component';

describe('OverlayCheckoutConfirmComponent', () => {
  let component: OverlayCheckoutConfirmComponent;
  let fixture: ComponentFixture<OverlayCheckoutConfirmComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OverlayCheckoutConfirmComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OverlayCheckoutConfirmComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
