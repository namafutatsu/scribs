import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 's-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit {
  @Input() title: string;
  
  constructor() { }

  ngOnInit() {}

}
