import { Component, OnInit, Input } from '@angular/core';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-action',
  templateUrl: './action.component.html',
  styleUrls: ['./action.component.scss'],
})
export class ActionComponent implements OnInit {
  @Input() context: WorkspaceContext;
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
