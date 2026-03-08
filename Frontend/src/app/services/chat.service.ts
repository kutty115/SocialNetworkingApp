import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

export interface ChatMessage {
  fromUserId: string;
  toUserId: string;
  text: string;
  sentAt: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private hub?: signalR.HubConnection;

  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  messages$ = this.messagesSubject.asObservable();

  private buffer: ChatMessage[] = [];

 start(): Promise<void> {
  if (this.hub) return Promise.resolve();

  this.hub = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7061/hubs/chat', {
      accessTokenFactory: () => localStorage.getItem('token') ?? ''
    })
    .withAutomaticReconnect()
    .build();

  this.hub.on('ReceiveMessage', (msg: ChatMessage) => {
    this.buffer = [...this.buffer, msg];
    this.messagesSubject.next(this.buffer);
  });

  return this.hub.start();
}

  stop(): Promise<void> {
    if (!this.hub) return Promise.resolve();
    const h = this.hub;
    this.hub = undefined;
    return h.stop();
  }

  clearMessages() {
    this.buffer = [];
    this.messagesSubject.next([]);
  }

  async send(toUserId: string, text: string) {
    if (!this.hub) await this.start();
    return this.hub!.invoke('SendMessage', toUserId, text);
  }
}