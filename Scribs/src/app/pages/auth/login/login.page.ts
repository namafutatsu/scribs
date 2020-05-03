import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
})
export class LoginPage implements OnInit {
  loading = false;
  credentialsForm: FormGroup;
 
  constructor(private formBuilder: FormBuilder, private authService: AuthService) { }
 
  ngOnInit() {
    this.credentialsForm = this.formBuilder.group({
      name: ['user'],
      password: ['password', [Validators.required, Validators.minLength(6)]]
    });
  }
 
  onSubmit() {
    this.loading = true;
    this.authService.login(this.credentialsForm.value).subscribe(() => this.loading = false);
  }
}
