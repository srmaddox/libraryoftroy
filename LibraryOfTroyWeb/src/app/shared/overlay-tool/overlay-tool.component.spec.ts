import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OverlayToolComponent } from './overlay-tool.component';

describe('OverlayToolComponent', () => {
  let component: OverlayToolComponent;
  let fixture: ComponentFixture<OverlayToolComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OverlayToolComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OverlayToolComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
