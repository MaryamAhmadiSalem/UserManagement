import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EMPTY, Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  http = inject(HttpClient);
  router = inject(Router);
  isSessionWarningShown = false;

  public postUsers(createUser: ICreateUser): Observable<IUser> {
    return this.http.post<IUser>('https://localhost:7073/api/users', createUser);
  }

  public getUsers(): Observable<IUser[]> {
    return this.http.get<IUser[]>('https://localhost:7073/api/users');
  }

  public getUserById(id: number): Observable<IUser> {
    return this.http.get<IUser>(`https://localhost:7073/api/users/${id}`);
  }

  public putUsers(id: number, user: IUser): Observable<IUser> {
    return this.http.put<IUser>(`https://localhost:7073/api/users/${id}`, user);
  }

  public deleteUser(id: number): Observable<IUser> {
    return this.http.delete<IUser>(`https://localhost:7073/api/users/${id}`);
  }

  public getUserByNationalId(nationalId: string): Observable<IUser> {
    return this.http.get<IUser>(`https://localhost:7073/api/users/search/${nationalId}`);
  }

  public registerUser(registerInfo: IRegister): Observable<IAuthToken> {
    return this.http.post<IAuthToken>('https://localhost:7073/api/register/user', registerInfo);
  }

  public loginUser(loginInfo: IRegister): Observable<IAuthToken> {
    return this.http.post<IAuthToken>('https://localhost:7073/api/login/user', loginInfo);
  }

  public assignRole(assignInfo: IAssignRole):Observable<void>{
    return this.http.post<void>('https://localhost:7073/api/users/assign-role', assignInfo);
  }

  public saveToken(token: IAuthToken): void {
    localStorage.setItem('accessToken', token.accessToken);
    localStorage.setItem('refreshToken', token.refreshToken);
  }

  public removeToken() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  public postToken(): Observable<void> {
    var refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken === null) return EMPTY;
    return this.http.post<void>('https://localhost:7073/api/auth/logout', { token: refreshToken });
  }

  public postExpiredToken(): Observable<void> {
    var refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken === null) return EMPTY;
    return this.http.post<void>('https://localhost:7073/api/auth/refresh-expired', {
      token: refreshToken,
    });
  }

  public renewToken(): Observable<IAuthToken> {
    var refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken === null) return EMPTY;
    return this.http
      .post<IAuthToken>('https://localhost:7073/api/token/renewal', { token: refreshToken })
      .pipe(tap((newToken) => this.saveToken(newToken)));
  }

  public getTokenExpiration() {
    const accessToken = localStorage.getItem('accessToken');
    if (accessToken === null) return null;
    const decoded = jwtDecode(accessToken);
    const exp = decoded.exp;
    if (exp == null) return null;
    const remainingTime = exp * 1000 - Date.now();
    if (remainingTime <= 0) return 'TokenExpired';
    return remainingTime;
  }

  public checkTokenExpiration(): void {
    const remainingTime = this.getTokenExpiration();
    if (remainingTime === null) return;

    if (remainingTime === 'TokenExpired') {
      this.logoutAfterExpiration();
      return;
    }
    if (remainingTime <= 60000) {
      this.handleSessionWarning();
      return;
    }
    setTimeout(() => this.checkTokenExpiration(), remainingTime - 60000);
  }

  public getTokenRole(): string | null {
    var accessToken = localStorage.getItem('accessToken');
    if (accessToken === null) return null;
    const decoded = jwtDecode<IUserToken>(accessToken);
    const role = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    return role ?? null;
  }
  public isManager(): boolean {
    var role = this.getTokenRole();
    if (role === 'Manager') return true;
    return false;
  }

  private handleSessionWarning(): void {
    if(this.isSessionWarningShown) return ;
    this.isSessionWarningShown = true;
    const result = confirm(
      'Your session is about to end. Would you like to continue using your account?',
    );
    if (result) {
      this.renewToken().subscribe({
        next: () => {
          this.isSessionWarningShown = false;
          this.checkTokenExpiration()
        },
        error: () => this.logoutAfterExpiration(),
      });
    } else this.logoutAfterExpiration();
  }

  private logoutAfterExpiration(): void {
    this.postExpiredToken().subscribe({
      next: () => this.finishLogout(),
      error: () => this.finishLogout(),
    });
  }

  private finishLogout(): void {
    this.removeToken();
    this.router.navigate(['/register']);
  }
}
