import { AfterViewInit, Component, OnInit, Input, ViewChild } from '@angular/core';
import { ModalController } from '@ionic/angular';

import { Document } from 'src/app/models/workspace';
import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';
import { NamingComponent } from '../../naming/naming.component';

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
    idField: 'tempId',
    hasChildrenField: 'isDirectory'
  };
  
  constructor(public modalController: ModalController) { }

  ngOnInit() {
    this.nodes = [this.context.workspace.project];
  }

  ngAfterViewInit() {
    this.tree.treeModel.expandAll();
  }

  onActivated(event) {
    this.context.open = event.node.id;
  }

  touch() {
    this.context.syncProject = true;
  }

  onUpdateData(event) {
    this.touch();
  }

  onMoveNode(event) {
    this.touch();
  }

  generateDocument(isLeaf: boolean, name: string): Document {
    var document = new Document();
    document.tempId = this.createTempId();
    document.name = name;
    document.isLeaf = isLeaf;
    document.isDirectory = !isLeaf;
    document.children = [];
    return document;
  }

  insertBrother(parent: Document, active: Document, document: Document) {
    const index = parent.children.indexOf(active);
    if (index === parent.children.length - 1) {
      parent.children.push(document) 
    } else {
      this.insertChild(parent, document, index);
    }
  }

  insertChild(parent: Document, document: Document, index: number) {
    let newChildren = parent.children.slice(0, index + 1);
    newChildren.push(document);
    newChildren = newChildren.concat(parent.children.slice(index + 1));
    parent.children = newChildren;
  }

  insertDocumentInContext(document: Document, parent: Document) {
    this.context.documents[document.tempId] = document;
    this.context.parents[document.tempId] = parent;
    this.context.workspace.texts[document.tempId] = "";
    this.touch();
  }

  createDocument(isLeaf: boolean, name: string) {
    const document = this.generateDocument(isLeaf, name);
    let active = this.context.workspace.project;
    if (this.context.open) {
      active = this.context.documents[this.context.open];
    }
    let parent = null;
    if (active.isLeaf) {
      parent = this.context.parents[active.tempId];
      this.insertBrother(parent, active, document);
    } else {
      parent = active;
      parent.children.push(document) 
    }
    this.insertDocumentInContext(document, parent);
    this.tree.treeModel.update();
  }

  onCreateDirectory() {
    this.namingModal(false);
  }

  onCreateLeaf() {
    this.namingModal(true);
  }

  async namingModal(isLeaf: boolean) {
    const modal = await this.modalController.create({
      component: NamingComponent,
      cssClass: 'naming-modal'
    });
    modal.present();
    const dataFromModal = await modal.onWillDismiss();
    const name = dataFromModal.data.value['name'];
    this.createDocument(isLeaf, name);
  }

  createTempId() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      var r = Math.random() * 16 | 0,
        v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
