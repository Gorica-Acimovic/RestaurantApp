export interface ReviewDto {
  id: number;
  reservationId: number;
  guestName: string;
  visitDate: string;
  rating: number;
  comment: string | null;
  createdAt: string;
  managerResponse: string | null;
  respondedAt: string | null;
  responderName: string | null;
  rowVersion: string;
}

export interface ReviewSummaryDto {
  averageRating: number;
  reviewCount: number;
  latestReviews: ReviewDto[];
}

export interface CreateReviewRequest {
  reservationId: number;
  rating: number;
  comment: string | null;
}

export interface RespondToReviewRequest {
  response: string;
  rowVersion: string;
}