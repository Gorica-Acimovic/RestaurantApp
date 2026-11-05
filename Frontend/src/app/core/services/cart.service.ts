import { Injectable } from '@angular/core';
import { CartLine } from '../models/online-order.models';
import { MenuItemDto } from '../models/menu.models';

const CART_STORAGE_KEY = 'cart';
const MAX_QUANTITY = 99;

@Injectable({
  providedIn: 'root'
})
export class CartService {
  lines: CartLine[] = [];

  constructor() {
    const text = sessionStorage.getItem(CART_STORAGE_KEY);
    this.lines = text ? (JSON.parse(text) as CartLine[]) : [];
   }

  get count() : number {
    return this.lines.reduce((sum, line) => sum + line.quantity, 0);
  }

  get subtotal() : number {
    return this.lines.reduce((sum, line) => sum + line.price * line.quantity, 0);
  }

  add(item: MenuItemDto) : void {
    const line = this.lines.find(l => l.menuItemId === item.id);

    if(line) {
      line.quantity = Math.min(line.quantity + 1, MAX_QUANTITY);
    } else {
      this.lines.push({
        menuItemId: item.id,
        name: item.name,
        price: item.price,
        quantity: 1
      });
    }
    this.save();

  }

  setQuantity(line: CartLine, quantity: number): void {
    if (quantity < 1) {
      this.remove(line);
      return;
    }

    line.quantity = Math.min(quantity, MAX_QUANTITY);
    this.save();
  }

   remove(line: CartLine): void {
    this.lines = this.lines.filter(l => l !== line);
    this.save();
  }

  clear(): void {
    this.lines = [];
    this.save();
  }



   private save(): void {
    sessionStorage.setItem(CART_STORAGE_KEY, JSON.stringify(this.lines));
  }

}
