import { Component, OnInit } from '@angular/core';
import { MenuService } from '../../services/menu.service';
import { FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MenuGroup, MenuItemDto } from '../../models/menu.models';
import { CURRENCY } from '../../currency';
import { DecimalPipe } from '@angular/common';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime } from 'rxjs';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [
   ReactiveFormsModule,
    DecimalPipe,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    RouterLink
  ],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent  implements OnInit {
   items: MenuItemDto[] = [];
  loaded = false;
  errorMessage = '';
  readonly currency = CURRENCY;
  groups: MenuGroup[]=[];

  searchControl = new FormControl('', { nonNullable: true });
  categoryControl = new FormControl('', { nonNullable: true });


  constructor(private menuService: MenuService, private formBuilder: FormBuilder, private toastr: ToastrService  ) { }

   ngOnInit(): void {
    this.loadItems();

    this.searchControl.valueChanges
      .pipe(debounceTime(300))
      .subscribe(() => this.loadItems());

    this.categoryControl.valueChanges.subscribe(() => this.loadItems());
  }

  loadItems(): void {
    this.errorMessage = '';

    this.menuService
      .getMenuItems(this.searchControl.value, this.categoryControl.value)
      .subscribe({
        next: (data) => {
          this.items = data;
          this.groups = this.buildGroups(data);
          this.loaded = true;
        },
        error: () => (this.errorMessage = 'Could not load the menu.')
      });
  }

  private buildGroups(items: MenuItemDto[]): MenuGroup[] {
  const groups: MenuGroup[] = [];

  for (const item of items) {
    let group = groups.find(g => g.subcategoryId === item.subcategoryId);

    if (!group) {
      group = { subcategoryId: item.subcategoryId, name: item.subcategoryName, category: item.category, items: [] };
      groups.push(group);
    }

    group.items.push(item);
  }

  return groups;
}

}
