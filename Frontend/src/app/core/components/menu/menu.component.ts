import { Component, OnInit } from '@angular/core';
import { MenuService } from '../../services/menu.service';
import { FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MenuItemDto } from '../../models/menu.models';
import { CURRENCY } from '../../currency';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime } from 'rxjs';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [
   ReactiveFormsModule,
    RouterLink,
    DecimalPipe,
    MatToolbarModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent  implements OnInit {
   items: MenuItemDto[] = [];
  loaded = false;
  errorMessage = '';
  readonly currency = CURRENCY;

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
          this.loaded = true;
        },
        error: () => (this.errorMessage = 'Could not load the menu.')
      });
  }

}
