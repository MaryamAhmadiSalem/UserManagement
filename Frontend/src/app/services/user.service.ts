import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  http = inject(HttpClient);

  public postUsers(createUser: ICreateUser): Observable<IUser> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.post<IUser>('https://localhost:7073/api/users', createUser, { headers });
  }

  public getUsers(): Observable<IUser[]> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.get<IUser[]>('https://localhost:7073/api/users', { headers });
  }

  public getUserById(id: number): Observable<IUser> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.get<IUser>(`https://localhost:7073/api/users/${id}`, { headers });
  }

  public putUsers(id: number, user: IUser): Observable<IUser> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.put<IUser>(`https://localhost:7073/api/users/${id}`, user, { headers });
  }

  public deleteUser(id: number): Observable<string> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.delete<string>(`https://localhost:7073/api/users/${id}`, { headers });
  }

  public getUserByNationalId(nationalId: string): Observable<IUser> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.get<IUser>(`https://localhost:7073/api/users/search/${nationalId}`, {
      headers,
    });
  }

  public registerUser(registerInfo: IRegister): Observable<string> {
    return this.http.post<string>('https://localhost:7073/api/register/user', registerInfo);
  }

  public loginUser(loginInfo: IRegister): Observable<string> {
    return this.http.post<string>('https://localhost:7073/api/login/user', loginInfo);
  }
  public saveToken(token: string): void {
    localStorage.setItem('token', token);
  }
  public removeToken() {
    localStorage.removeItem('token');
  }
  public postToken(): Observable<void> {
    var token = localStorage.getItem('token');
     const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem('token')}`,
    });
    return this.http.post<void>('https://localhost:7073/api/auth/logout', token, {headers});
  }
}
