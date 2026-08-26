import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface NewsItem {
  readonly id: string;
  readonly title: string;
  readonly summary: string;
  readonly content: string;
  readonly publishedAt: string;
  readonly validFrom: string;
  readonly validUntil: string | null;
}

export interface NewsPage {
  readonly items: readonly NewsItem[];
  readonly page: number;
  readonly pageSize: number;
  readonly total: number;
  readonly hasMore: boolean;
}

@Injectable({ providedIn: 'root' })
export class NewsApiService {
  private readonly http = inject(HttpClient);

  listPage(page: number, pageSize: number): Observable<NewsPage> {
    return this.http.get<NewsPage>('/api/news', { params: { page, pageSize } });
  }
}
