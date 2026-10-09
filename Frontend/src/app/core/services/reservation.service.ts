import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_URL } from '../api-config';
import { ReservationDto, ReservationRequest, ChangeReservationStatusRequest } from '../models/reservation.models';
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

  changeStatus(id:number, request: ChangeReservationStatusRequest):Observable<ReservationDto>{
    return this.http.put<ReservationDto>(`${API_URL}/Reservations/${id}/status`,request)
  }
}
