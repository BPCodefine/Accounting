// src/app/services/user.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private http: HttpClient) {}

  getUsername(): Observable<{ username: string }> {
    return this.http.get<{ username: string }>('/api/username', {withCredentials: true});
  }
}
