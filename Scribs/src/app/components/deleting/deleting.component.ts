import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';

@Component({
  selector: 'app-deleting',
  templateUrl: './deleting.component.html',
  styleUrls: ['./deleting.component.scss'],
})
export class DeletingComponent implements OnInit {
  private form : FormGroup;

  constructor(public modalController: ModalController, private formBuilder: FormBuilder) {
    this.form = this.formBuilder.group({
      name: ['', Validators.required]
    });
  }

  ngOnInit() {}

  isValid() {
    return this.form.value['name'] === 'DELETE';
  }

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
