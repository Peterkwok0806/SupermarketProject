import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChatResponse {
  success: boolean;
  message: string;
  sessionId?: string;
  item?: any;
}

export interface ChatMessageDto {
  id: number;
  role: string;
  content: string;
  createdAt: Date;
}

export interface ChatSessionSummaryDto {
  sessionId: string;
  createdAt: Date;
  lastActivityAt: Date;
  messageCount: number;
  lastMessagePreview?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiChatService {

  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}api/chat`;
  private localStorageKey = 'ai_chat_session_id';
  private maxAnonymousSessions = 5;

  /**
   * 取得當前 anonymous session ID（從 localStorage）
   */
  getCurrentSessionId(): string | null {
    return localStorage.getItem(this.localStorageKey);
  }

  /**
   * 設定當前 anonymous session ID
   */
  setCurrentSessionId(sessionId: string): void {
    localStorage.setItem(this.localStorageKey, sessionId);
  }

  /**
   * 清除 anonymous session ID
   */
  clearSessionId(): void {
    localStorage.removeItem(this.localStorageKey);
  }

  /**
   * 發送聊天訊息（支援 SessionId）
   */
  sendChatMessage(message: string, sessionId?: string): Observable<ChatResponse> {
    const body: any = { message };
    if (sessionId) {
      body.sessionId = sessionId;
    }
    return this.http.post<ChatResponse>(this.apiUrl, body);
  }

  /**
   * 取得聊天歷史
   */
  getChatHistory(sessionId: string): Observable<{ success: boolean; item: ChatMessageDto[] }> {
    return this.http.get<{ success: boolean; item: ChatMessageDto[] }>(`${this.apiUrl}/history/${sessionId}`);
  }

  /**
   * 取得使用者所有 Sessions（需登入）
   */
  getUserSessions(): Observable<{ success: boolean; item: ChatSessionSummaryDto[] }> {
    return this.http.get<{ success: boolean; item: ChatSessionSummaryDto[] }>(`${this.apiUrl}/sessions`);
  }

  /**
   * 刪除 Session
   */
  deleteSession(sessionId: string): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.apiUrl}/session/${sessionId}`);
  }

  /**
   * 管理 anonymous sessions（localStorage）
   * 當超過 maxAnonymousSessions 時，刪除最舊的
   */
  manageAnonymousSessions(): void {
    const sessionsJson = localStorage.getItem('ai_chat_sessions');
    let sessions: string[] = sessionsJson ? JSON.parse(sessionsJson) : [];

    if (sessions.length >= this.maxAnonymousSessions) {
      const oldestSessionId = sessions.shift();
      if (oldestSessionId) {
        this.deleteSession(oldestSessionId).subscribe();
      }
    }
  }
}
