import { Component, OnInit } from '@angular/core';
import { MenuService } from '../../services/menu.service';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MenuItemDto, MenuSubcategoryDto } from '../../models/menu.models';
import { CURRENCY } from '../../currency';
import { DecimalPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-menu-management',
  standalone: true,
  imports: [
     ReactiveFormsModule,
    DecimalPipe,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatTableModule
  ],
  templateUrl: './menu-management.component.html',
  styleUrl: './menu-management.component.css'
})
export class MenuManagementComponent  implements OnInit {
items: MenuItemDto[] = [];
  editingItem: MenuItemDto | null = null;
  errorMessage = '';
  saveError = '';
  form: FormGroup;
  readonly currency = CURRENCY;
  subcategories: MenuSubcategoryDto[] = [];
  displayedColumns = ['name', 'category', 'subcategory', 'price', 'status', 'actions'];

constructor(private menuService: MenuService, private formBuilder: FormBuilder, private toastr: ToastrService) {
  this.form = this.formBuilder.group({
  name: ['', [Validators.required, Validators.maxLength(100)]],
  description: ['', [Validators.maxLength(500)]],
  price: [null, [Validators.required, Validators.min(0)]],
  category: ['Food'],
  subcategoryId: [null, [Validators.required]]
});
}



  ngOnInit(): void {
  this.loadMenuItems();
  this.menuService.getSubcategories().subscribe({
    next: (data) => (this.subcategories = data),
    error: () => (this.errorMessage = 'Failed to load subcategories.')
  });
}

  loadMenuItems(): void {
  this.menuService.getMenuItems('', '').subscribe({
    next:(data) => (this.items = data),
    error:() => (this.errorMessage='Failed to load menu items. Please try again later.')

  });
}

  save(directive: FormGroupDirective): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saveError = '';
    const { name, description, price, subcategoryId } = this.form.getRawValue();
const request = { name, description, price, subcategoryId };

    if (this.editingItem) {
      this.menuService.updateMenuItem(this.editingItem.id, request).subscribe({
        next: () => {
          this.toastr.success('Menu item updated.');
          this.finishSave(directive);
        },
        error: (err) => {
          this.saveError = err.error?.message ?? 'Something went wrong. Please try again.';
        }
      });
    } else {
      this.menuService.createMenuItem(request).subscribe({
        next: () => {
          this.toastr.success('Menu item added.');
          this.finishSave(directive);
        },
        error: (err) => {
          this.saveError = err.error?.message ?? 'Something went wrong. Please try again.';
        }
      });
    }
  }

  startEdit(item: MenuItemDto): void {
    this.editingItem = item;
    this.saveError = '';
    this.form.setValue({
      name: item.name,
      description: item.description,
      price: item.price,
      category: item.category,
      subcategoryId: item.subcategoryId
    });
  }

  cancelEdit(directive: FormGroupDirective): void {
    this.editingItem = null;
    this.saveError = '';
    directive.resetForm(this.emptyForm());
  }

  toggleAvailability(item: MenuItemDto): void {
    this.menuService.setAvailability(item.id, !item.isAvailable).subscribe({
      next: () => this.loadMenuItems(),
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  removeItem(item: MenuItemDto, directive: FormGroupDirective): void {
    if (!confirm(`Remove "${item.name}" from the menu?`)) {
      return;
    }

    this.menuService.deleteMenuItem(item.id).subscribe({
      next: () => {
        this.toastr.success('Menu item removed.');

        if (this.editingItem?.id === item.id) {
          this.cancelEdit(directive);
        }

        this.loadMenuItems();
      },
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  private finishSave(directive: FormGroupDirective): void {
    this.editingItem = null;
    directive.resetForm(this.emptyForm());
    this.loadMenuItems();
  }

  private emptyForm() {
  return { name: '', description: '', price: null, category: 'Food', subcategoryId: null };  }

  filteredSubcategories(): MenuSubcategoryDto[] {
  return this.subcategories.filter(s => s.category === this.form.controls['category'].value);
}

onCategoryChange(): void {
  this.form.controls['subcategoryId'].setValue(null);
}
}


  

