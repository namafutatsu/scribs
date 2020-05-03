
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastController, AlertController } from '@ionic/angular';
import { Storage } from '@ionic/storage';

import { AuthService } from 'src/app/services/auth.service';
import { ProjectService } from 'src/app/services/project.service';

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
    private projectService: ProjectService, private storage: Storage, private toastController: ToastController,
    private alertController: AlertController) { }
 
  ngOnInit() {
    this.projectCreationForm = this.formBuilder.group({
      name: ['', [Validators.required]],//(Enter a name)
      // password: ['password', [Validators.required, Validators.minLength(6)]]
    });
    this.loadProjects();
  }

  loadProjects() {
    this.projectService.getList().subscribe(res => {
      this.projects = res;
    });
  }

  onCreateStart(event) {
    event.stopPropagation();
    this.creation = true;
  }

  private showAlert(msg) {
    let alert = this.alertController.create({
      message: msg,
      header: 'Error',
      buttons: ['OK']
    });
    alert.then(alert => alert.present());
  }
 
  onSubmit(event) {
    this.loading = true
    event.stopPropagation();
    this.projectService.post(this.projectCreationForm.value).subscribe((workspace: any) => {
      this.loading = false;
      this.goTo(workspace);
    }, () => {
      this.showAlert('A project with the same name already exists');
      this.loading = false;
    });
  }

  goTo(workspace) {
    this.router.navigate([`/workspace/${workspace.id}`]);
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
