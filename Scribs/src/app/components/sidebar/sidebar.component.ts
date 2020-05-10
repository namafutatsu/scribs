import { Component, OnInit } from '@angular/core';

import { ActionContext } from './action/action.component';

@Component({
  selector: 's-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent implements OnInit {
  actionContext: ActionContext;

  constructor() {
    this.actionContext = new ActionContext(null);
  }

  ngOnInit() {}
  
}
