import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChatResponse {
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiChatService {

  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}api/chat`;

  sendChatMessage(message: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(this.apiUrl, { message });
  }
}
