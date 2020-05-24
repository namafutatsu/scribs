import { AfterViewInit, Component, OnInit, Input, ViewChild } from '@angular/core';
import { ModalController } from '@ionic/angular';

import { Document } from 'src/app/models/workspace';
import { WorkspaceContext } from 'src/app/pages/workspace/workspace.page';
import { NamingComponent } from '../../naming/naming.component';
import { DeletingComponent } from '../../deleting/deleting.component';

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
    this.context.parents[event.node.tempId] = event.to.parent;
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

  insertLastChild(parent: Document, document: Document) {
    parent.children.push(document) 
  }

  insertDocumentInContext(document: Document, parent: Document) {
    this.context.documents[document.tempId] = document;
    this.context.parents[document.tempId] = parent;
    this.context.workspace.texts[document.tempId] = "";
    this.touch();
  }

  getActive() {
    if (this.context.open) {
      return this.context.documents[this.context.open];
    }
    return this.context.workspace.project;
  }

  getActiveParent(document: Document) {
    if (document.isLeaf) {
      return this.context.parents[document.tempId];
    } 
    return document;
  }

  createDocument(isLeaf: boolean, name: string) {
    const document = this.generateDocument(isLeaf, name);
    const active = this.getActive();
    let parent = this.getActiveParent(active);
    if (active.isLeaf) {
      this.insertBrother(parent, active, document);
    } else {
      this.insertLastChild(parent, document);
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

  canRename() {
    return this.context.open && this.context.open != this.context.workspace.project.tempId;
  }

  canDelete() {
    return this.context.open && this.context.open != this.context.workspace.project.tempId;
  }

  deleteChild(parent: Document, document: Document) {
    const index = parent.children.indexOf(document);
    if (index === 0) {
      parent.children.shift();
    } else if (index === parent.children.length - 1) {
      parent.children.pop();
    } else {
      parent.children = parent.children.slice(0, index).concat(parent.children.slice(index + 1));
    }
  }

  delete() {
    const active = this.getActive();
    const parent = this.context.parents[active.tempId];
    this.deleteChild(parent, active);
    this.tree.treeModel.update();
  }

  onDelete() {
    this.deletingModal();
  }

  async deletingModal() {
    const modal = await this.modalController.create({
      component: DeletingComponent,
      cssClass: 'deleting-modal'
    });
    modal.present();
    const dataFromModal = await modal.onWillDismiss();
    const name = dataFromModal.data.value['name'];
    this.delete();
  }

  createTempId() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      var r = Math.random() * 16 | 0,
        v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
