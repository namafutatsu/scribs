import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { AuthorizedService } from './authorized.service';
import { Text } from '../models/workspace';

@Injectable({
  providedIn: 'root'
})
export class TextService extends AuthorizedService {
  protected controller = 'Text';

  public postText(text: Text): Observable<string> {
    return this.post<string>(text, 'post');
  }
}
