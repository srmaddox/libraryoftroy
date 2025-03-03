import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PersonalBorrowsComponent } from './personal-borrows.component';

describe('PersonalBorrowsComponent', () => {
  let component: PersonalBorrowsComponent;
  let fixture: ComponentFixture<PersonalBorrowsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PersonalBorrowsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PersonalBorrowsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
