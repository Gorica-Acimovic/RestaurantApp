export type PaymentMethod = 'Cash' | 'Card';
export type OrderStatus = 'Open' | 'Closed' | 'Cancelled';

export interface OrderItemDto {
  id: number;
  menuItemId: number;
  menuItemName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  note: string | null;
}

export interface OrderDto {
  id: number;
  tableId: number;
  tableNumber: number;
  waiterName: string;
  openedAt: string;
  closedAt: string | null;
  status: OrderStatus;
  numberOfGuests: number;
  items: OrderItemDto[];
  total: number;
}

export interface OpenOrderRequest {
  tableId: number;
  numberOfGuests: number;
}

export interface AddOrderItemRequest {
  menuItemId: number;
  quantity: number;
  note: string | null;
}

export interface UpdateOrderItemRequest {
  quantity: number;
  note: string | null;
}

export interface CloseOrderRequest {
  paymentMethod: PaymentMethod;
}

export interface InvoiceDto {
  id: number;
  invoiceNumber: number;
  invoiceNumberText: string;
  orderId: number;
  tableNumber: number;
  waiterName: string;
  issuedAt: string;
  paymentMethod: PaymentMethod;
  items: OrderItemDto[];
  totalAmount: number;
}