import { ErrorHandler, Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { NotificationService } from './notification.service';
import { LoggerService } from './logger.service';

/**
 * Angular 全域例外處理器
 *
 * 捕獲所有未被元件或服務 catch 的例外，
 * 使用 LoggerService 記錄詳細資訊（帶顏色 + 時間戳），
 * 並透過 NotificationService 顯示使用者友善的 toast 訊息。
 *
 * 註冊方式：app.config.ts → providers: [{ provide: ErrorHandler, useClass: GlobalErrorHandler }]
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private notification = inject(NotificationService);
  private logger = inject(LoggerService);

  handleError(error: any): void {
    // 優先處理 HttpErrorResponse（Angular HTTP 攔截器拋出的例外）
    if (error instanceof HttpErrorResponse) {
      this.handleHttpError(error);
      return;
    }

    // 一般 JS 錯誤（元件模板錯誤、未捕獲的 Promise rejection 等）
    this.logger.error('未捕獲的應用例外', error);

    // 顯示通用訊息給使用者
    if (error?.message?.includes('Loading chunk')) {
      // Webpack chunk 載入失敗（可能是新版本部署後舊快取仍存在）
      this.notification.error('應用程式已更新，請重新整理頁面');
    } else {
      this.notification.error('發生未預期的錯誤，請稍後再試');
    }
  }

  private handleHttpError(error: HttpErrorResponse): void {
    switch (error.status) {
      case 0:
        this.logger.error('無法連線到伺服器，請檢查網路連線');
        this.notification.error('無法連線到伺服器，請檢查網路');
        break;

      case 400:
        this.logger.error('請求參數錯誤 (400)', error.message);
        // 不顯示 toast，因為 400 通常由元件自行處理（例如表單驗證失敗）
        break;

      case 401:
        this.logger.error('未授權 (401)：登入已過期或無效');
        // 401 由 AuthInterceptor 處理（自動刷新 token），不需重複顯示
        break;

      case 403:
        this.logger.error('拒絕存取 (403)：您沒有權限執行此操作');
        this.notification.error('您沒有權限執行此操作');
        break;

      case 404:
        this.logger.error('資源不存在 (404)', error.url);
        // 404 通常由元件自行處理
        break;

      case 429:
        this.logger.error('請求過於頻繁 (429)：已觸發速率限制');
        this.notification.error('請求過於頻繁，請稍後再試');
        break;

      default:
        if (error.status >= 500) {
          this.logger.error(`伺服器錯誤 (${error.status})`, error.message);
          this.notification.error('伺服器發生錯誤，請稍後再試');
        } else {
          this.logger.error(`HTTP 錯誤 (${error.status})`, error.message);
        }
        break;
    }
  }
}
