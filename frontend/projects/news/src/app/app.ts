import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  inject,
  signal,
} from '@angular/core';

import { LocaleDatePipe } from './i18n/locale-date.pipe';
import { LocaleService } from './i18n/locale.service';
import { NewsApiService, NewsItem } from './news-api.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-root',
  imports: [LocaleDatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements AfterViewInit, OnDestroy {
  private readonly api = inject(NewsApiService);
  private readonly seenIds = new Set<string>();
  private observer?: IntersectionObserver;
  private nextPage = 1;

  protected readonly localeService = inject(LocaleService);
  protected readonly items = signal<NewsItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal(false);
  protected readonly hasMore = signal(true);

  @ViewChild('sentinel') private sentinel?: ElementRef<HTMLElement>;

  ngAfterViewInit(): void {
    this.loadNextPage();

    const element = this.sentinel?.nativeElement;
    if (!element || typeof IntersectionObserver === 'undefined') return;

    this.observer = new IntersectionObserver((entries) => {
      if (entries.some((entry) => entry.isIntersecting)) {
        this.loadNextPage();
      }
    });
    this.observer.observe(element);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  protected retry(): void {
    this.error.set(false);
    this.loadNextPage();
  }

  private loadNextPage(): void {
    if (this.loading() || !this.hasMore() || this.error()) return;
    this.loading.set(true);

    this.api.listPage(this.nextPage, PAGE_SIZE).subscribe({
      next: (page) => {
        const additions = page.items.filter((item) => !this.seenIds.has(item.id));
        additions.forEach((item) => this.seenIds.add(item.id));
        this.items.update((current) => [...current, ...additions]);
        this.hasMore.set(page.hasMore);
        this.nextPage += 1;
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
      },
    });
  }
}
