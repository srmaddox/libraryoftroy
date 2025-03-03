import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BorrowedBookManagerComponent } from './borrowed-book-manager.component';

describe('BorrowedBookManagerComponent', () => {
  let component: BorrowedBookManagerComponent;
  let fixture: ComponentFixture<BorrowedBookManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BorrowedBookManagerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BorrowedBookManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
