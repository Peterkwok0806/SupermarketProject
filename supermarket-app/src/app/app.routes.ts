import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { CartComponent } from './components/cart/cart.component';
import { RegisterComponent } from './components/auth/register/register.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'cart', component: CartComponent },
    { path: 'register', component: RegisterComponent },

    { path: '**', redirectTo: '' }

];
