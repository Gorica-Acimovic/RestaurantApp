export type KitchenItemStatus = 'New' | 'Preparing' | 'Ready';

export interface KitchenItemDto {
  id: number;
  name: string;
  quantity: number;
  note: string | null;
  status: KitchenItemStatus;
}

export interface KitchenTicketDto {
  orderId: number;
  channel: string;
  title: string;
  note: string | null;
  since: string;
  items: KitchenItemDto[];
}