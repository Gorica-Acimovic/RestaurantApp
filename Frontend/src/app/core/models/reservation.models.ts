export interface ReservationsDto {
    id:number,
    customerName:string,
    customerEmail:string,
    customerPhone:string,
    startsAt:Date,
    endsAt:Date,
    status:string,
    tableId:number,
    tableNumber:number,
    rowVersion:string

}

export interface ReservationRequest{
    customerName:string,
    customerEmail:string,
    customerPhone:String,
    startsAt:Date,
    numberOfGuests:number

}