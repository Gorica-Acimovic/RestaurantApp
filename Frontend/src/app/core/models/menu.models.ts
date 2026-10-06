export type MenuCategory = 'Food' | 'Drink';

export interface MenuItemDto {
  id: number;
  name: string;
  description: string;
  price: number;
  category: MenuCategory;
  isAvailable: boolean;
}

export interface SaveMenuItemRequest {
  name: string;
  description: string;
  price: number;
  category: MenuCategory;
}