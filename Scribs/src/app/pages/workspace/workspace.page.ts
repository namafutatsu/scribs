import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProjectService } from 'src/app/services/project.service';
import { Document, Text, Workspace } from 'src/app/models/workspace';
import { TextService } from 'src/app/services/text.service';

export class WorkspaceContext {
  public workspace: Workspace;
  public documents = {};
  public parents = {};
  public action: string;
  public open: string;
  public syncProject = false;
  public syncTexts = {};
  public uploading = {};
  public isUploading = false;
  public isDisconnected = false;

  constructor(action: string) {
    this.action = action;
  }
}

@Component({
  selector: 's-workspace',
  templateUrl: './workspace.page.html',
  styleUrls: ['./workspace.page.scss'],
})
export class WorkspacePage implements OnInit {
  context: WorkspaceContext;
  projectId: string;
  workspace: Workspace;

  constructor(private route: ActivatedRoute, private projectService: ProjectService, private textService: TextService) {
    this.context = new WorkspaceContext('explorer');
  }

  ngOnInit() {
    this.projectId = this.route.snapshot.paramMap.get('id');
    this.projectService.getProject(this.projectId).subscribe(res => {
      this.context.workspace = res;
      this.workspace = res;
      this.getDocuments(this.context.workspace.project, null);
    });
  }

  getDocuments(document: Document, parent: Document) {
    this.context.documents[document.tempId] = document;
    this.context.parents[document.tempId] = parent;
    if (document.children !== undefined && document.children !== null) {
      for (const index in document.children) {
        this.getDocuments(document.children[index], document);
      }
    }
  }

  markUploading(id: string) {
    this.context.uploading[id] = true;
    this.context.isUploading = true;
  }

  unmarkUploading(id: string) {
    delete this.context.uploading[id];
    if (Object.keys(this.context.uploading).length == 0) {
      this.context.isUploading = false;
    }
  }

  updateProject() {
    const id = "project";
    if (this.context.syncProject === true) {
      this.context.syncProject = false;
      this.markUploading(id);
      this.projectService.postProject(this.context.workspace.project).subscribe(
        res => {
          this.context.isDisconnected = false;
        },
        err => {
          this.context.isDisconnected = true;
          this.context.syncProject = true;
        },
        () => {
          this.unmarkUploading(id);
        }
      );
    }
  }

  updateTexts() {
    const syncTexts = {};
    for (const id in this.context.syncTexts) {
      syncTexts[id] = true;
    }
    for (const id in syncTexts) {
      delete this.context.syncTexts[id];
      this.markUploading(id);
      const text = new Text();
      text.id = id;
      text.content = this.context.workspace.texts[id];
      this.textService.postText(text).subscribe(
        res => {
          this.context.isDisconnected = false;
        },
        err => {
          this.context.isDisconnected = true;
          this.context.syncTexts[id] = true;
        },
        () => {
          this.unmarkUploading(id);
        }
      );
    }
  }

  onSaving() {
    this.updateProject();
    this.updateTexts();
  }

}