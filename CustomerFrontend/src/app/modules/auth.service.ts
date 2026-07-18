import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  isAuthenticated = signal(false);
  userEmail = signal('');

  constructor() {
    const token = localStorage.getItem('Mytoken');
    const email = localStorage.getItem('userEmail');
    if (token) {
      this.isAuthenticated.set(true);
    }
    if (email) {
      this.userEmail.set(email);
    }
  }

  setAuthenticated(value: boolean) {
    this.isAuthenticated.set(value);
  }

  setUserEmail(email: string) {
    this.userEmail.set(email);
    localStorage.setItem('userEmail', email);
  }

  getUserEmail() {
    return this.userEmail();
  }

  getIsAuthenticated() {
    return this.isAuthenticated();
  }
}
