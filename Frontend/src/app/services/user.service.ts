import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  http = inject(HttpClient);

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
}
