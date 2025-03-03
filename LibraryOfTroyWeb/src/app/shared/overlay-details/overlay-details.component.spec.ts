import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OverlayDetailsComponent } from './overlay-details.component';

describe('OverlayDetailsComponent', () => {
  let component: OverlayDetailsComponent;
  let fixture: ComponentFixture<OverlayDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OverlayDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OverlayDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
