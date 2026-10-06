export type TableStatus = 'Available' | 'Occupied';

export interface TableDto {
    id: number;
    number: number;
    capacity: number;
    status:TableStatus;
    positionX: number;
    positionY: number;
   
}

export interface SaveTableRequest {
    number: number;
    capacity: number;
    positionX: number;
    positionY: number;
}

export interface UpdateTablePositionRequest {
    positionX: number;
    positionY: number;
}