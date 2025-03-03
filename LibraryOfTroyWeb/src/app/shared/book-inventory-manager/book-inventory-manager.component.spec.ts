import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BookInventoryManagerComponent } from './book-inventory-manager.component';

describe('BookInventoryManagerComponent', () => {
  let component: BookInventoryManagerComponent;
  let fixture: ComponentFixture<BookInventoryManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookInventoryManagerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BookInventoryManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
