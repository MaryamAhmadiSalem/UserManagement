import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/user.service';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-assignment-role',
  styleUrl: './assignment-role.component.css',
  templateUrl: './assignment-role.component.html',
})
export class AssignmentRole {
  private activatedRoute = inject(ActivatedRoute);
  private service = inject(UserService);
  private router = inject(Router);
  id: number | null = null;
  userId: string | null = null;

  labels: string[] = ['firstName', 'lastName', 'nationalId'];
  options: string[] = ['', 'Admin', 'User'];
  assigntRoleForm: FormGroup = new FormGroup({
    firstName: new FormControl(''),
    lastName: new FormControl(''),
    nationalId: new FormControl(''),
    role: new FormControl('', [Validators.required]),
  });
  ngOnInit() {
    var routeId = this.activatedRoute.snapshot.paramMap.get('id');
    if (routeId != null) {
      this.id = Number(routeId);
      this.service.getUserById(this.id).subscribe({
        next: (res) => {
          this.userId = res.userId;
          this.assigntRoleForm.patchValue({
            firstName: res.firstName,
            lastName: res.lastName,
            nationalId: res.nationalId,
          });
        },
        error: (err) => console.error(err),
      });
    }
  }
  public onSubmit() {
    if (this.assigntRoleForm.valid) {
      if (this.userId != null)
        this.service
          .assignRole({
            roleName: this.assigntRoleForm.value.role,
            userId: this.userId,
          })
          .subscribe({
            next: () => this.router.navigate(['/mainlayout/users']),
          });
    }
  }
}
