import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

/**
 * 統一日誌服務 — 帶有時間戳記與顏色標示
 *
 * Development 環境：所有等級都輸出到 console（帶顏色）
 * Production 環境：僅 error 輸出（方便串接 Sentry / 後端日誌 API）
 *
 * Console 樣式：
 *   [13:01:25.432] 🟢 [LOG]  偵測到已登入        → 綠色
 *   [13:01:26.100] 🟡 [WARN] Token 刷新中         → 黃色
 *   [13:01:27.500] 🔴 [ERROR] 登入失敗             → 紅色
 */
@Injectable({ providedIn: 'root' })
export class LoggerService {
  private isDev = !environment.production;

  // ========== CSS Color Constants ==========
  private static readonly GREEN  = 'color: #22c55e; font-weight: bold';   // green-500
  private static readonly YELLOW = 'color: #eab308; font-weight: bold';   // yellow-500
  private static readonly RED    = 'color: #ef4444; font-weight: bold';   // red-500
  private static readonly GRAY   = 'color: #9ca3af';                     // gray-400

  /**
   * 一般日誌（Development 環境才輸出，綠色標示）
   */
  log(message: string, ...args: any[]) {
    if (!this.isDev) return;
    console.log(
      `%c[${this.timestamp()}] %c[LOG]%c ${message}`,
      LoggerService.GRAY, LoggerService.GREEN, '', ...args
    );
  }

  /**
   * 警告日誌（Development 環境才輸出，黃色標示）
   */
  warn(message: string, ...args: any[]) {
    if (!this.isDev) return;
    console.warn(
      `%c[${this.timestamp()}] %c[WARN]%c ${message}`,
      LoggerService.GRAY, LoggerService.YELLOW, '', ...args
    );
  }

  /**
   * 錯誤日誌（所有環境都輸出，紅色標示）
   * Production 可在此串接 Sentry 或後端日誌 API
   */
  error(message: string, ...args: any[]) {
    console.error(
      `%c[${this.timestamp()}] %c[ERROR]%c ${message}`,
      LoggerService.GRAY, LoggerService.RED, '', ...args
    );
  }

  /**
   * 產生時間戳記字串：HH:mm:ss.SSS
   * 例如：13:01:25.432
   */
  private timestamp(): string {
    const now = new Date();
    const hh = String(now.getHours()).padStart(2, '0');
    const mm = String(now.getMinutes()).padStart(2, '0');
    const ss = String(now.getSeconds()).padStart(2, '0');
    const ms = String(now.getMilliseconds()).padStart(3, '0');
    return `${hh}:${mm}:${ss}.${ms}`;
  }
}
