import { Component, OnInit, Input } from '@angular/core';
import { ActionContext } from '../action/action.component';

@Component({
  selector: 's-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
})
export class ExplorerComponent implements OnInit {
  @Input() context: ActionContext;

  constructor() { }

  ngOnInit() {}

}
