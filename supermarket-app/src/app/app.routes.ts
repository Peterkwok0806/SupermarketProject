import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { CartComponent } from './components/cart/cart.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { LoginComponent } from './components/auth/login/login.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'cart', component: CartComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'login', component: LoginComponent },

    { path: '**', redirectTo: '' }

];
