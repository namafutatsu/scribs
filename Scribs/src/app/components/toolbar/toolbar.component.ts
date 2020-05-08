import { Component, OnInit, Input } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { SettingsComponent } from '../settings/settings.component';

@Component({
  selector: 's-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit {
  @Input() title: string;
  settings = false;
  
  constructor(public modalController: ModalController) { }

  ngOnInit() {}

  openSettings() {
    this.settings = true;
  }

  async presentModal() {
    const modal = await this.modalController.create({
      component: SettingsComponent
    });
    return await modal.present();
  }
}
