import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { CartComponent } from './components/cart/cart.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { LoginComponent } from './components/auth/login/login.component';
import { authGuard } from './guards/auth.guard';
import { CheckoutComponent } from './components/checkout/checkout.component';
import { OrderSuccessComponent } from './components/order-success/order-success.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'cart', component: CartComponent, canActivate: [authGuard] },
    { path: 'register', component: RegisterComponent },
    { path: 'login', component: LoginComponent },
    { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard]  },
    { path: 'order-success', component: OrderSuccessComponent },

    { path: '**', redirectTo: '' }

];
