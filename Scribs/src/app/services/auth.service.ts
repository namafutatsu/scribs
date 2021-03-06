import { Platform, AlertController } from '@ionic/angular';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Storage } from '@ionic/storage';
import { tap, catchError } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';

import { environment } from '../../environments/environment';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  TOKEN_KEY = 'access_token';
 
  url = environment.url;
  token = null;
  user = null;
  authenticationState = new BehaviorSubject(false);
 
  constructor(private http: HttpClient, private router: Router, private helper: JwtHelperService,
    private storage: Storage, private plt: Platform, private alertController: AlertController) {
    this.plt.ready().then(() => {
      this.checkToken();
    });
  }
 
  checkToken() {
    this.storage.get(this.TOKEN_KEY).then(token => {
      if (token) {
        let decoded = this.helper.decodeToken(token);
        let isExpired = this.helper.isTokenExpired(token);
 
        if (!isExpired) {
          this.token = token;
          this.user = decoded;
          this.authenticationState.next(true);
        } else {
          this.storage.remove(this.TOKEN_KEY);
        }
      }
    });
  }
 
  register(credentials) {
    return this.http.post(`${this.url}/api/user/register`, credentials).pipe(
      catchError(e => {
        this.showAlert(e.error.msg);
        throw new Error(e);
      })
    );
  }
 
  login(credentials) {
    return this.http.post(`${this.url}/api/user/login`, credentials)
      .pipe(
        tap(res => {
          this.token = res['token'];
          this.storage.set(this.TOKEN_KEY, this.token);
          this.user = this.helper.decodeToken(this.token);
          this.authenticationState.next(true);
          // this.router.navigate(['projects']);
        }),
        catchError(e => {
          this.showAlert(e.error.msg);
          throw new Error(e);
        })
      );
  }
 
  logout() {
    this.storage.remove(this.TOKEN_KEY).then(() => {
      this.authenticationState.next(false);
    });
  }
 
  isAuthenticated() {
    return this.authenticationState.value;
  }
 
  showAlert(msg) {
    let alert = this.alertController.create({
      message: msg,
      header: 'Error',
      buttons: ['OK']
    });
    alert.then(alert => alert.present());
  }
}