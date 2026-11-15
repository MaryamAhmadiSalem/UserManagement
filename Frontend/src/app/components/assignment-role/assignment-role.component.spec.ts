import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssignmentRole } from './assignment-role.component';

describe('AssignmentRole', () => {
  let component: AssignmentRole;
  let fixture: ComponentFixture<AssignmentRole>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssignmentRole],
    }).compileComponents();

    fixture = TestBed.createComponent(AssignmentRole);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
