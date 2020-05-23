import { Injectable } from '@angular/core';

import { AuthorizedService } from './authorized.service';
import { Observable } from 'rxjs';
import { Workspace, Project } from '../models/workspace';
import { WorkspaceContext } from '../pages/workspace/workspace.page';

@Injectable({
  providedIn: 'root'
})
export class ProjectService extends AuthorizedService {
  protected controller = 'Project';
  
  public getList() {
    return this.get('getlist');
  }

  public getProject(id: string): Observable<Workspace> {
    return this.post<Workspace>({ Id: id }, 'get');
  }

  public postProject(project: Project): Observable<Project> {
    return this.post<Project>(project, 'post');
  }
}
