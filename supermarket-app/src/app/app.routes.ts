import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { ProductlistComponent } from './components/productlist/productlist.component';
import { CartComponent } from './components/cart/cart.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { LoginComponent } from './components/auth/login/login.component';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';
import { CheckoutComponent } from './components/checkout/checkout.component';
import { OrderSuccessComponent } from './components/order-success/order-success.component';
import { OrdersComponent } from './components/orders/orders.component';
import { OrderDetailComponent } from './components/order-detail/order-detail.component';
import { ProfileComponent } from './components/profile/profile.component';





export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'products', component: ProductlistComponent },
    { 
      path: 'product/:id', 
      loadComponent: () => import('./components/product-detail/product-detail.component').then(m => m.ProductDetailComponent) 
    },
    { path: 'cart', component: CartComponent, canActivate: [authGuard] },
    { path: 'register', component: RegisterComponent },
    { path: 'login', component: LoginComponent },
    { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard]  },
    { path: 'order-success', component: OrderSuccessComponent, canActivate: [authGuard] },
    { path: 'orders', component: OrdersComponent, canActivate: [authGuard] },
    { path: 'order/:snowflakeId', component: OrderDetailComponent, canActivate: [authGuard] },
    { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
    { 
      path: 'admin', 
      //canActivate: [authGuard, adminGuard],
      loadChildren: () => import('./admin.routes').then(m => m.adminRoutes) 
    },

    { path: '**', redirectTo: '' }

];

