import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProjectService } from 'src/app/services/project.service';

export class WorkspaceContext {
  public action: string;
  public workspace: any;
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
  workspace;

  constructor(private route: ActivatedRoute, private projectService: ProjectService) {
    this.context = new WorkspaceContext(null);
  }

  ngOnInit() {
    this.projectId = this.route.snapshot.paramMap.get('id');
    this.projectService.getProject(this.projectId).subscribe(res => {
      this.workspace = res;
      this.context.workspace = res;
    });
  }

}
