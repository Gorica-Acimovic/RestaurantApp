import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MenuItemDto, SaveMenuItemRequest } from '../models/menu.models';
import { API_URL } from '../api-config';

@Injectable({
  providedIn: 'root'
})
export class MenuService {

  constructor(private http: HttpClient) { }

  getMenuItems(search:string, category:string): Observable<MenuItemDto[]> {
    let params = new HttpParams();

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    if (category) {
      params = params.set('category', category);
    }

    return this.http.get<MenuItemDto[]>(`${API_URL}/MenuItems`, { params });
  }

   createMenuItem(request: SaveMenuItemRequest): Observable<MenuItemDto> {
    return this.http.post<MenuItemDto>(`${API_URL}/MenuItems`, request);
  }

  updateMenuItem(id: number, request: SaveMenuItemRequest): Observable<void> {
    return this.http.put<void>(`${API_URL}/MenuItems/${id}`, request);
  }

  setAvailability(id: number, isAvailable: boolean): Observable<void> {
    return this.http.put<void>(`${API_URL}/MenuItems/${id}/availability`, { isAvailable });
  }

  deleteMenuItem(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/MenuItems/${id}`);
  }

}
