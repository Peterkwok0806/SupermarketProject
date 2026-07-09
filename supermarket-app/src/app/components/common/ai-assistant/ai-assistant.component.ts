import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiChatService, ChatMessageDto, ChatSessionSummaryDto } from '../../../services/ai-chat.service';

interface ChatMessage {
  sender: 'user' | 'ai';
  text: string;
}

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-assistant.component.html',
  styleUrl: './ai-assistant.component.css'
})
export class AiAssistantComponent {

  private aiChatService = inject(AiChatService);

  isOpen = signal(false);
  messages = signal<ChatMessage[]>([
    { sender: 'ai', text: '你好！我是您的超級市場智慧助理，您可以問我任何關於商品價格、庫存或促銷的問題喔！' }
  ]);
  userQuery = signal('');
  isLoading = signal(false);
  currentSessionId = signal<string | null>(null);
  sessions = signal<ChatSessionSummaryDto[]>([]);
  showSessions = signal(false);
  view = signal<'chat' | 'sessions'>('chat');

  constructor() {
    // 載入 anonymous session ID
    const savedSessionId = this.aiChatService.getCurrentSessionId();
    if (savedSessionId) {
      this.currentSessionId.set(savedSessionId);
      this.loadSessionHistory(savedSessionId);
    }
  }

  toggleChat(): void {
    this.isOpen.update(v => !v);
    if (this.isOpen()) {
      this.loadUserSessions();
    }
  }

  sendMessage(): void {
    const query = this.userQuery().trim();
    if (!query || this.isLoading()) return;

    // 加入使用者訊息
    this.messages.update(msgs => [...msgs, { sender: 'user', text: query }]);
    this.userQuery.set('');
    this.isLoading.set(true);

    this.aiChatService.sendChatMessage(query, this.currentSessionId() ?? undefined).subscribe({
      next: (res) => {
        if (res.success && res.item) {
          // 回應格式為 { sessionId, response }
          const aiText = res.item.response || '抱歉，目前無法回覆您的問題，請稍後再試。';
          this.messages.update(msgs => [...msgs, { sender: 'ai', text: aiText }]);

          // 儲存或更新 SessionId
          if (res.item.sessionId) {
            const isNewSession = !this.currentSessionId();
            this.currentSessionId.set(res.item.sessionId);
            this.aiChatService.setCurrentSessionId(res.item.sessionId);

            if (isNewSession) {
              this.aiChatService.manageAnonymousSessions();
            }
          }
        } else {
          this.messages.update(msgs => [...msgs, { sender: 'ai', text: res.message || '抱歉，目前無法回覆您的問題，請稍後再試。' }]);
        }
      },
      error: () => {
        this.messages.update(msgs => [...msgs, {
          sender: 'ai',
          text: '⚠️ 連線發生錯誤，請確認網路或稍後再試。'
        }]);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  startNewChat(): void {
    this.currentSessionId.set(null);
    this.aiChatService.clearSessionId();
    this.messages.set([
      { sender: 'ai', text: '你好！我是您的超級市場智慧助理，您可以問我任何關於商品價格、庫存或促銷的問題喔！' }
    ]);
    this.view.set('chat');
  }

  loadSessionHistory(sessionId: string): void {
    this.isLoading.set(true);
    this.aiChatService.getChatHistory(sessionId).subscribe({
      next: (res) => {
        if (res.success && res.item) {
          const chatMessages: ChatMessage[] = res.item.map((msg: ChatMessageDto) => ({
            sender: msg.role === 'User' ? 'user' as const : 'ai' as const,
            text: msg.content
          }));
          this.messages.set(chatMessages.length > 0 ? chatMessages : [
            { sender: 'ai', text: '你好！我是您的超級市場智慧助理，您可以問我任何關於商品價格、庫存或促銷的問題喔！' }
          ]);
        }
      },
      error: () => {
        console.error('Failed to load chat history');
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  loadUserSessions(): void {
    this.aiChatService.getUserSessions().subscribe({
      next: (res) => {
        if (res.success && res.item) {
          this.sessions.set(res.item);
        }
      },
      error: () => {
        // 未登入時忽略錯誤
        this.sessions.set([]);
      }
    });
  }

  openSession(sessionId: string): void {
    this.currentSessionId.set(sessionId);
    this.aiChatService.setCurrentSessionId(sessionId);
    this.view.set('chat');
    this.loadSessionHistory(sessionId);
  }

  deleteSession(sessionId: string, event: Event): void {
    event.stopPropagation();
    this.aiChatService.deleteSession(sessionId).subscribe({
      next: () => {
        this.sessions.update(s => s.filter(sess => sess.sessionId !== sessionId));
        if (this.currentSessionId() === sessionId) {
          this.startNewChat();
        }
      },
      error: () => {
        console.error('Failed to delete session');
      }
    });
  }

  toggleSessionsView(): void {
    this.view.update(v => v === 'chat' ? 'sessions' : 'chat');
    if (this.view() === 'sessions') {
      this.loadUserSessions();
    }
  }
}
