import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';

import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
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
    expect(newsLink?.textContent).toContain('News');
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
});
