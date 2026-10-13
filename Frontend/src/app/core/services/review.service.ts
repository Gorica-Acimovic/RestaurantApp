import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../api-config';
import { CreateReviewRequest, RespondToReviewRequest, ReviewDto, ReviewSummaryDto } from '../models/review.models';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {

  constructor(private http: HttpClient) { }

  getSummary(take: number): Observable<ReviewSummaryDto> {
    const params = new HttpParams().set('take', take);
    return this.http.get<ReviewSummaryDto>(`${API_URL}/Reviews/summary`, { params });
  }

  getReviews(rating: number | null, unanswered: boolean): Observable<ReviewDto[]> {
    let params = new HttpParams();

    if (rating !== null) {
      params = params.set('rating', rating);
    }

    if (unanswered) {
      params = params.set('unanswered', true);
    }

    return this.http.get<ReviewDto[]>(`${API_URL}/Reviews`, { params });
  }

  createReview(request: CreateReviewRequest): Observable<ReviewDto> {
    return this.http.post<ReviewDto>(`${API_URL}/Reviews`, request);
  }

  respond(id: number, request: RespondToReviewRequest): Observable<ReviewDto> {
    return this.http.put<ReviewDto>(`${API_URL}/Reviews/${id}/response`, request);
  }
}