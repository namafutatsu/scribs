
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastController } from '@ionic/angular';
import { Storage } from '@ionic/storage';

import { AuthService } from 'src/app/services/auth.service';
import { ProjectService } from 'src/app/services/project.service';
import { timeout } from 'rxjs/operators';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.page.html',
  styleUrls: ['./projects.page.scss'],
})
export class ProjectsPage implements OnInit {
  projects = null;
  creation = false;
  loading = false;
  projectCreationForm: FormGroup;
 
  constructor(private formBuilder: FormBuilder, private authService: AuthService, private router: Router,
    private projectService: ProjectService, private storage: Storage, private toastController: ToastController) { }
 
  ngOnInit() {
    this.projectCreationForm = this.formBuilder.group({
      name: [''],//(Enter a name)
      // password: ['password', [Validators.required, Validators.minLength(6)]]
    });
    this.loadProjects();
  }

  loadProjects() {
    this.projectService.getList().subscribe(res => {
      this.projects = res;
    });
  }

  onCreate(event) {
    event.stopPropagation();
    this.creation = true;
  }
 
  onSubmit(event) {
    this.loading = true
    event.stopPropagation();
    this.projectService.post(this.projectCreationForm.value).subscribe((res: any) => {
      this.router.navigate([`/workspace/${res.id}`])
    }, () => {
      this.loading = false
    });
  }
 
  onCancel(event) {
    event.stopPropagation();
    this.creation = false;
  }
 
  logout() {
    this.authService.logout();
  }
 
  clearToken() {
    // ONLY FOR TESTING!
    this.storage.remove('access_token');
 
    let toast = this.toastController.create({
      message: 'JWT removed',
      duration: 3000
    });
    toast.then(toast => toast.present());
  }

}
