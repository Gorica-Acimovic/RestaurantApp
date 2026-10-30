import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_URL } from '../api-config';
import { ReservationDto, ReservationRequest, ChangeReservationStatusRequest, AvailableTableDto } from '../models/reservation.models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {

  constructor(private http:HttpClient) { }

  createReservation(request:ReservationRequest):Observable<ReservationDto>{
    return this.http.post<ReservationDto>(`${API_URL}/Reservations`,request);
  }

  getReservations(date:string):Observable<ReservationDto[]>{
    let params = new HttpParams();

    if(date)
    {
      params = params.set('date',date);
    }
    return this.http.get<ReservationDto[]>(`${API_URL}/Reservations`, {params});
  }

    getMyReservations(): Observable<ReservationDto[]> {
    return this.http.get<ReservationDto[]>(`${API_URL}/Reservations/mine`);
  }

  changeStatus(id:number, request: ChangeReservationStatusRequest):Observable<ReservationDto>{
    return this.http.put<ReservationDto>(`${API_URL}/Reservations/${id}/status`,request)
  }
  getAvailableTables(startsAt: string, guestNumber: number): Observable<AvailableTableDto[]> {
  const params = new HttpParams().set('startsAt', startsAt).set('guestNumber', guestNumber);
  return this.http.get<AvailableTableDto[]>(`${API_URL}/Reservations/availability`, { params });
}

getSuggestedTable(id: number): Observable<AvailableTableDto> {
  return this.http.get<AvailableTableDto>(`${API_URL}/Reservations/${id}/suggested-table`);
}
}
