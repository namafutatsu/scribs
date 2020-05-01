import { ToastController } from '@ionic/angular';
import { Component, OnInit } from '@angular/core';
import { Storage } from '@ionic/storage';

import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.page.html',
  styleUrls: ['./projects.page.scss'],
})
export class ProjectsPage implements OnInit {
  workspaces = null;
 
  constructor(private authService: AuthService, private storage: Storage, private toastController: ToastController) { }
 
  ngOnInit() {
  }
 
  loadProjects() {
    this.authService.getProjects().subscribe(res => {
      this.workspaces = res;
    });
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
