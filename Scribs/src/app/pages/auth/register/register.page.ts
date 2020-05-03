import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss'],
})
export class RegisterPage implements OnInit {
  loading = false;
  credentialsForm: FormGroup;
 
  constructor(private formBuilder: FormBuilder, private authService: AuthService) {}
 
  ngOnInit() {
    this.credentialsForm = this.formBuilder.group({
      name: ['user'],
      mail: ['user@scribs.io', [Validators.required, Validators.email]],
      password: ['password', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['password', [Validators.required, Validators.minLength(6)]]
    });
  }
 
  onSubmit() {
    this.loading = true;
    this.authService.register(this.credentialsForm.value).subscribe(res => {
      this.authService.login(this.credentialsForm.value).subscribe(() => this.loading = false);
    });
  }
}
