import { Component, OnInit } from '@angular/core';
import { CartService } from '../../services/cart.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Router, RouterLink } from '@angular/router';
import { OnlineOrderService } from '../../services/online-order.service';
import { CreateOnlineOrderRequest, OrderChannel } from '../../models/online-order.models';
import { CURRENCY } from '../../currency';
import { DecimalPipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DecimalPipe,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
form:FormGroup;
deliveryFee = 0;
submitting = false;
readonly currency = CURRENCY;


constructor(
  public cartService: CartService,
  private onlineOrderService: OnlineOrderService, 
  private formBuilder: FormBuilder, 
  private router: Router,
  private toastr: ToastrService) { 

  this.form = this.formBuilder.group({
    channel:['Pickup'],
    phone: ['', [Validators.required, Validators.pattern(/^[0-9+/ -]{6,20}$/)]],
    address: ['', [Validators.maxLength(200)]],
    note: ['', [Validators.maxLength(200)]],
    paymentMethod: ['Cash']
  });

}


  ngOnInit(): void {
    this.onlineOrderService.getSettings().subscribe({
      next: (settings) => ( this.deliveryFee = settings.deliveryFee)
    });

    this.form.controls['channel'].valueChanges.subscribe((channel: OrderChannel) =>
      this.applyChannel(channel));
  }

  get isDelivery(): boolean {
    return this.form.value.channel === 'Delivery';
  }

  get fee(): number {
    return this.isDelivery ? this.deliveryFee : 0;
  }

  get total(): number {
    return this.cartService.subtotal + this.fee;
  }

  private applyChannel(channel: OrderChannel) : void  {
    const address = this.form.controls['address'];

    if(channel == 'Delivery') {
      address.setValidators([Validators.required, Validators.maxLength(200)]);
    } else {
      address.setValidators([Validators.maxLength(200)]);
    }
    address.updateValueAndValidity();
  }

  submit(): void {
     if (this.cartService.lines.length === 0) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

     const value = this.form.getRawValue();

     const request : CreateOnlineOrderRequest = {
      channel: value.channel,
      phone: value.phone.trim(),
      address: this.isDelivery ? value.address.trim() : null,
      note: value.note.trim() || null,
      paymentMethod: value.paymentMethod,
      items: this.cartService.lines.map(line => ({
        menuItemId: line.menuItemId,
        quantity: line.quantity,
        note:null
      }))

  };

    this.submitting = true;
    this.onlineOrderService.createOrder(request).subscribe({
      next: () => {
        this.cartService.clear();
        this.toastr.success('Order created successfully');
        this.router.navigate(['/dashboard/my-orders']);
      },
      error: (err) => {
        this.submitting = false;
        this.toastr.error(err?.error?.message || 'Something went wrong. Please try again later.');
      }
    });
  }
}
  
