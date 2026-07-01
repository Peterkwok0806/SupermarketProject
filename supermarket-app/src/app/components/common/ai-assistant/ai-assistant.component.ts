import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiChatService } from '../../../services/ai-chat.service';

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

  toggleChat(): void {
    this.isOpen.update(v => !v);
  }

  sendMessage(): void {
    const query = this.userQuery().trim();
    if (!query || this.isLoading()) return;

    // 加入使用者訊息
    this.messages.update(msgs => [...msgs, { sender: 'user', text: query }]);
    this.userQuery.set('');
    this.isLoading.set(true);

    this.aiChatService.sendChatMessage(query).subscribe({
      next: (res) => {
        const aiText = res.success
          ? res.message
          : '抱歉，目前無法回覆您的問題，請稍後再試。';
        this.messages.update(msgs => [...msgs, { sender: 'ai', text: aiText }]);
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
}
