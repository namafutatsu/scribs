import { Component, OnInit, Input } from '@angular/core';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
})
export class ExplorerComponent implements OnInit {
  @Input() context: WorkspaceContext;

  constructor() { }

  ngOnInit() {}

}
