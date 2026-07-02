import { ApplicationConfig, provideZoneChangeDetection, importProvidersFrom, ErrorHandler } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';
import { MatIconModule } from '@angular/material/icon';
import { GlobalErrorHandler } from './services/global-error-handler';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'top' })), 
    provideHttpClient(withInterceptors([authInterceptor])),
    importProvidersFrom(MatIconModule),
    // 全域例外處理：取代 Angular 預設的 ConsoleErrorHandler
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ]
};
