import { Injectable } from '@angular/core';

import { AuthorizedService } from './authorized.service';

@Injectable({
  providedIn: 'root'
})
export class ProjectService extends AuthorizedService {
  protected controller = 'Project';
  
  public getList() {
      return this.get('getlist');
  }
}
