import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { LocaleService } from '../i18n/locale.service';
import { NewsFeedComponent } from './news-feed';
import { NewsPage } from './news-api.service';

describe('NewsFeedComponent', () => {
  let httpMock: HttpTestingController;
  let localeService: LocaleService;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [NewsFeedComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    localeService = TestBed.inject(LocaleService);
    localeService.locale.set('de');
  });

  afterEach(() => {
    httpMock.verify();
  });

  function expectFirstPageRequest(): ReturnType<HttpTestingController['expectOne']> {
    return httpMock.expectOne(
      (request) =>
        request.url === '/api/news' &&
        request.params.get('page') === '1' &&
        request.params.get('pageSize') === '20',
    );
  }

  it('should create the component', () => {
    const fixture = TestBed.createComponent(NewsFeedComponent);
    fixture.detectChanges();
    expectFirstPageRequest().flush(emptyPage());

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders the loaded News items', () => {
    const fixture = TestBed.createComponent(NewsFeedComponent);
    fixture.detectChanges();

    expectFirstPageRequest().flush({
      items: [
        {
          id: '00000000-0000-0000-0000-000000000001',
          title: 'Willkommen',
          summary: 'Die Praxis-Website ist im Aufbau.',
          content: 'Die Praxis-Website ist im Aufbau.',
          publishedAt: '2026-08-21T08:00:00Z',
          validFrom: '2026-08-21T08:00:00Z',
          validUntil: null,
        },
      ],
      page: 1,
      pageSize: 20,
      total: 1,
      hasMore: false,
    } satisfies NewsPage);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.news-card h3')?.textContent).toContain('Willkommen');
  });

  it('shows a retry action when loading fails', () => {
    const fixture = TestBed.createComponent(NewsFeedComponent);
    fixture.detectChanges();

    expectFirstPageRequest().flush('boom', { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.status.error')).toBeTruthy();

    (compiled.querySelector('.status.error button') as HTMLButtonElement).click();
    expectFirstPageRequest().flush(emptyPage());
  });

  it('reacts to a locale change', () => {
    const fixture = TestBed.createComponent(NewsFeedComponent);
    fixture.detectChanges();
    expectFirstPageRequest().flush(emptyPage());

    localeService.setLocale('es');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toBe('Bienvenido a nuestra consulta');
  });
});

function emptyPage(): NewsPage {
  return { items: [], page: 1, pageSize: 20, total: 0, hasMore: false };
}
