import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgTemplateOutlet } from '@angular/common';

import { LocaleService } from './i18n/locale.service';
import { LOCALE_NAMES, Locale, SUPPORTED_LOCALES } from './i18n/translation.model';
import { NAV_ITEMS } from './nav-items';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgTemplateOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly localeService = inject(LocaleService);
  protected readonly localeNames = LOCALE_NAMES;
  protected readonly supportedLocales = SUPPORTED_LOCALES;
  protected readonly navItems = NAV_ITEMS;
  protected readonly navOpen = signal(false);
  protected readonly langMenuOpen = signal(false);

  protected toggleNav(): void {
    this.navOpen.update((open) => !open);
  }

  protected closeNav(): void {
    this.navOpen.set(false);
    this.langMenuOpen.set(false);
  }

  protected toggleLangMenu(): void {
    this.langMenuOpen.update((open) => !open);
  }

  protected selectLocale(locale: Locale): void {
    this.localeService.setLocale(locale);
    this.langMenuOpen.set(false);
  }
}
