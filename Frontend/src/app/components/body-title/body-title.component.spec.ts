import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BodyTitle } from './body-title.component';

describe('BodyTitle', () => {
  let component: BodyTitle;
  let fixture: ComponentFixture<BodyTitle>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BodyTitle],
    }).compileComponents();

    fixture = TestBed.createComponent(BodyTitle);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
