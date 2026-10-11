export type MenuCategory = 'Food' | 'Drink';

export interface MenuItemDto {
  id: number;
  name: string;
  description: string;
  price: number;
  category: MenuCategory;
  isAvailable: boolean;
  subcategoryId: number;
  subcategoryName: string;
}

export interface MenuSubcategoryDto {
  id: number;
  name: string;
  category: MenuCategory;
  sortOrder: number;
}

export interface MenuGroup {
  subcategoryId: number;
  name: string;
  category: MenuCategory;
  items: MenuItemDto[];
}

export interface SaveMenuItemRequest {
  name: string;
  description: string;
  price: number;
  subcategoryId: number;
}