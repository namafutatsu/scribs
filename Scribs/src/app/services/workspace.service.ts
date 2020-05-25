import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { AuthorizedService } from './authorized.service';
import { Workspace } from '../models/workspace';

@Injectable({
  providedIn: 'root'
})
export class WorkspaceService extends AuthorizedService {
  protected controller = 'Workspace';

  public getWorkspace(id: string): Observable<Workspace> {
    return this.post<Workspace>({ Id: id }, 'get');
  }
}
