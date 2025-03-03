import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibraryBagComponent } from './library-bag.component';

describe('LibraryBagComponent', () => {
  let component: LibraryBagComponent;
  let fixture: ComponentFixture<LibraryBagComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LibraryBagComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LibraryBagComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
