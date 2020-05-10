import { Component, OnInit, Input } from '@angular/core';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.scss'],
})
export class EditorComponent implements OnInit {
  @Input() context: WorkspaceContext;

  constructor() { }

  ngOnInit() {}

}
