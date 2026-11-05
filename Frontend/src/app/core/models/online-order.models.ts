import { OrderItemDto, PaymentMethod } from './order.models';

export type OrderChannel = 'DineIn' | 'Pickup' | 'Delivery';

export type DeliveryStatus =
  | 'Received'
  | 'Confirmed'
  | 'Preparing'
  | 'Ready'
  | 'OutForDelivery'
  | 'Delivered'
  | 'PickedUp'
  | 'Cancelled';

export interface OnlineOrderItemRequest {
  menuItemId: number;
  quantity: number;
  note: string | null;
}

export interface CreateOnlineOrderRequest {
  channel: OrderChannel;
  phone: string;
  address: string | null;
  note: string | null;
  paymentMethod: PaymentMethod;
  items: OnlineOrderItemRequest[];
}

export interface OnlineOrderDto {
  id: number;
  channel: OrderChannel;
  status: DeliveryStatus;
  placedAt: string;
  address: string | null;
  phone: string;
  note: string | null;
  paymentMethod: PaymentMethod;
  courierName: string | null;
  guestName: string | null;
  items: OrderItemDto[];
  subtotal: number;
  deliveryFee: number;
  total: number;
}

export interface OnlineOrderSettings {
  deliveryFee: number;
}

export interface CartLine {
  menuItemId: number;
  name: string;
  price: number;
  quantity: number;
}