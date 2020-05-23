import { AfterViewInit, Component, OnInit, Input, ViewChild } from '@angular/core';

import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';

@Component({
  selector: 's-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
})
export class ExplorerComponent implements AfterViewInit, OnInit {
  @Input() context: WorkspaceContext;
  @ViewChild('tree', {static: false}) tree;
  nodes;
  options = {
    allowDrag: true,
    allowDrop: (element, { parent, index }) => {
      return !parent.isLeaf;
    },
    hasChildrenField: 'isNode'
  };
  
  constructor() { }

  ngOnInit() {
    this.nodes = this.context.workspace.project.children;
  }

  ngAfterViewInit() {
    this.tree.treeModel.expandAll();
  }

  onActivated(event) {
    this.context.open = event.node.id;
  }

  onUpdateData(event) {
    this.context.syncProject = true;
  }

  onMoveNode(event) {
    this.context.syncProject = true;
  }
}
