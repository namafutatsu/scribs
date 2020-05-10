import { Component, OnInit, Input } from '@angular/core';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss'],
})
export class MenuComponent implements OnInit {
  @Input() context: WorkspaceContext;

  constructor() { }

  ngOnInit() {}

}
