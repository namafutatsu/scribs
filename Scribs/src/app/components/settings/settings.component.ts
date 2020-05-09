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
    document.body.classList.toggle('dark', event.detail.value === 'dark');
  }

  save() {
    const toggle: any = document.querySelector('#themeToggle');
    this.theme = toggle.value;
    this.storage.set('theme', this.theme);
    this.dismiss();
  }

  cancel() {
    document.body.classList.toggle('dark', this.theme === 'dark');
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
