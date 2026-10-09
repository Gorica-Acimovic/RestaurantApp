export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'NoShow' | 'Completed';

export interface ReservationDto {
    id:number,
    customerName:string,
    customerEmail:string | null,
    customerPhone:string | null,
    startsAt:string,
    endsAt:string,
    numberOfGuests:number,
    status:ReservationStatus,
    tableId:number | null,
    tableNumber:number | null,
    rowVersion:string

}

export interface ReservationRequest{
    customerName:string,
    customerEmail:string | null,
    customerPhone:string | null,
    startsAt:string,
    numberOfGuests:number

}

export interface ChangeReservationStatusRequest{
    status:ReservationStatus,
    rowVersion:string
}