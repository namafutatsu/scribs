import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { AuthorizedService } from './authorized.service';
import { Workspace, Project } from '../models/workspace';

@Injectable({
  providedIn: 'root'
})
export class ProjectService extends AuthorizedService {
  protected controller = 'Project';
  
  public getList() {
    return this.get('getlist');
  }

  public postProject(project: Project): Observable<Project> {
    return this.post<Project>(project, 'post');
  }
}
