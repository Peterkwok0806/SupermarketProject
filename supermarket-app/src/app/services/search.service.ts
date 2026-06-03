import { Injectable,signal } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SearchService {

  searchInputValue = signal<string>('');

  searchTerms$ = new Subject<string>();

  triggerInput(term: string) {
    this.searchInputValue.set(term);
    this.searchTerms$.next(term);
  }

  constructor() { }
}
