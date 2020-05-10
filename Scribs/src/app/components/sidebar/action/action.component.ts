import { Component, OnInit, Input } from '@angular/core';

export class ActionContext {
  public action: string;
  constructor(action: string) {
    this.action = action;
  }
}

@Component({
  selector: 's-action',
  templateUrl: './action.component.html',
  styleUrls: ['./action.component.scss'],
})
export class ActionComponent implements OnInit {
  @Input() context: ActionContext;
  @Input() action: string;
  @Input() icon: string;

  constructor() { }

  ngOnInit() {}

  run() {
    if (this.context.action === this.action) {
      this.context.action = null;
    } else {
      this.context.action = this.action;
    }
  }
}
