import { Component, OnInit } from '@angular/core';
import { Storage } from '@ionic/storage';

import { AuthService } from 'src/app/services/auth.service';
import { ModalController } from '@ionic/angular';

@Component({
  selector: 's-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss'],
})
export class SettingsComponent implements OnInit {
  theme = 'light';

  constructor(public modalController: ModalController, private authService: AuthService, private storage: Storage) { }

  ngOnInit() {
    this.storage.get('theme').then((val) => {
      if (val === 'dark') {
        const toggle: any = document.querySelector('#themeToggle');
        toggle.value = 'dark';
      }
    });
  }

  segmentChanged(event) {
    this.theme = event.detail.value;
    document.body.classList.toggle('dark', this.theme === 'dark');
  }

  save() {
    this.storage.set('theme', this.theme);
    this.dismiss();
  }

  dismiss() {
    this.modalController.dismiss({
      'dismissed': true
    });
  }

  logout() {
    this.authService.logout();
    this.dismiss();
  }

}
