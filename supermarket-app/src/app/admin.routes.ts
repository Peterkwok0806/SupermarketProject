import { Routes } from '@angular/router';


export const adminRoutes: Routes =[
    {
        path: '', loadComponent: () => import('./components/admin/layout/layout.component').then(m => m.AdminLayoutComponent),
        children:[
            {
                path: 'dashboard',loadComponent: () => import('./components/admin/dashboard/dashboard.component').then(m => m.AdminDashboardComponent)
            },

            {
                path: 'products',loadComponent: () => import('./components/admin/products/products.component').then(m => m.AdminProductsComponent)
            },


            {
                path: 'orders',loadComponent: () => import('./components/admin/orders/orders.component').then(m => m.AdminOrdersComponent)
            },

            {
                path: 'users',loadComponent: () => import('./components/admin/users/users.component').then(m => m.AdminUsersComponent)
            },
        ]

    }
]

    