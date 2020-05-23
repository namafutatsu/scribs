import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProjectService } from 'src/app/services/project.service';
import { Workspace } from 'src/app/models/workspace';

export class WorkspaceContext {
  public workspace: Workspace;
  public action: string;
  public open: string;
  public syncProject = true;
  public syncTexts = {};

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

  constructor(private route: ActivatedRoute, private projectService: ProjectService) {
    this.context = new WorkspaceContext('explorer');
  }

  ngOnInit() {
    this.projectId = this.route.snapshot.paramMap.get('id');
    this.projectService.getProject(this.projectId).subscribe(res => {
      this.context.workspace = res;
      this.workspace = res;
    });
  }

  onSaving() {
    if (this.context.syncProject === true) {
      this.context.syncProject = false;
      this.projectService.postProject(this.context.workspace.project).subscribe(
        res => {
        },
        error => {
          this.context.syncProject = true;
        }
      );
    }
  }

}