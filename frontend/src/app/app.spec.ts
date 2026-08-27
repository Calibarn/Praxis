import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';

import { App } from './app';
import { LocaleService } from './i18n/locale.service';

describe('App', () => {
  let localeService: LocaleService;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
    localeService = TestBed.inject(LocaleService);
    localeService.locale.set('de');
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('renders the primary navigation with a link to News', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const newsLink = compiled.querySelector('a[href="/news"]');
    expect(newsLink?.textContent).toContain('Aktuelles');
  });

  it('starts with the navigation collapsed and expands on toggle', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const shell = fixture.nativeElement.querySelector('.app-shell') as HTMLElement;

    expect(shell.classList.contains('nav-open')).toBe(false);

    const toggle = fixture.nativeElement.querySelector('.nav-toggle') as HTMLButtonElement;
    toggle.click();
    fixture.detectChanges();

    expect(shell.classList.contains('nav-open')).toBe(true);
  });

  it('opens the language menu and switches the UI language', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('.lang-menu')).toBeNull();

    (compiled.querySelector('.lang-toggle') as HTMLButtonElement).click();
    fixture.detectChanges();

    const options = compiled.querySelectorAll('.lang-option');
    expect(options.length).toBe(4);

    const englishOption = Array.from(options).find((option) =>
      option.textContent?.includes('English (US)'),
    ) as HTMLButtonElement;
    englishOption.click();
    fixture.detectChanges();

    expect(localeService.locale()).toBe('en');
    expect(compiled.querySelector('.plaque-tagline')?.textContent).toBe('General medicine');
    expect(compiled.querySelector('.lang-menu')).toBeNull();
  });
});
