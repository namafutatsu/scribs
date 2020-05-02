import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { AlertController } from '@ionic/angular';
import { Storage } from '@ionic/storage';
import { catchError } from 'rxjs/operators';

import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable()
export abstract class AuthorizedService {
  protected abstract controller: string;

  constructor(private http: HttpClient, private authService: AuthService, private storage: Storage,
    private alertController: AlertController) {}

  private getOptions() {
    return {
      headers: new HttpHeaders()
      .set('Content-Type', 'application/json')
      .set('Authorization', 'Bearer ' + this.authService.token),
      params: new HttpParams()
    };
  }

  public get(action: string) {
    return this.http.get(`${environment.url}/api/${this.controller}/${action}`, this.getOptions()).pipe(
      catchError(e => {
        let status = e.status;
        if (status === 401) {
          this.showAlert('You are not authorized for this!');
          this.logout();
        }
        throw new Error(e);
      })
    )
  }

  private showAlert(msg) {
    let alert = this.alertController.create({
      message: msg,
      header: 'Error',
      buttons: ['OK']
    });
    alert.then(alert => alert.present());
  }
 
  logout() {
    this.storage.remove(this.authService.TOKEN_KEY).then(() => {
      this.authService.authenticationState.next(false);
    });
  }
}
