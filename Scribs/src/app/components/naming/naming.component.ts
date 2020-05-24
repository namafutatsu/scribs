import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';

@Component({
  selector: 'app-naming',
  templateUrl: './naming.component.html',
  styleUrls: ['./naming.component.scss'],
})
export class NamingComponent implements OnInit {
  private form : FormGroup;

  constructor(public modalController: ModalController, private formBuilder: FormBuilder) {
    this.form = this.formBuilder.group({
      name: ['', Validators.required]
    });
  }

  ngOnInit() {}

  submit() {
    this.modalController.dismiss({
      'value': this.form.value
    });
  }

  cancel() {
    this.dismiss();
  }

  dismiss() {
    this.modalController.dismiss({
      'dismissed': true
    });
  }
}
