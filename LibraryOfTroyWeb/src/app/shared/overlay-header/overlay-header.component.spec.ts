import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OverlayHeaderComponent } from './overlay-header.component';

describe('OverlayHeaderComponent', () => {
  let component: OverlayHeaderComponent;
  let fixture: ComponentFixture<OverlayHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OverlayHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OverlayHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
