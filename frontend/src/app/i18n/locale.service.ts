import { Injectable, signal } from '@angular/core';

import { DEFAULT_LOCALE, Locale, SUPPORTED_LOCALES, Translation } from './translation.model';
import { TRANSLATIONS } from './translations';

const STORAGE_KEY = 'praxis.locale';

function isSupportedLocale(value: string | null | undefined): value is Locale {
  return (SUPPORTED_LOCALES as readonly string[]).includes(value ?? '');
}

function readStoredLocale(): Locale | undefined {
  try {
    const value = localStorage.getItem(STORAGE_KEY);
    return isSupportedLocale(value) ? value : undefined;
  } catch {
    return undefined;
  }
}

function detectBrowserLocale(): Locale | undefined {
  const primary = navigator.language?.slice(0, 2).toLowerCase();
  return isSupportedLocale(primary) ? primary : undefined;
}

@Injectable({ providedIn: 'root' })
export class LocaleService {
  readonly locale = signal<Locale>(readStoredLocale() ?? detectBrowserLocale() ?? DEFAULT_LOCALE);

  translations(): Translation {
    return TRANSLATIONS[this.locale()];
  }

  setLocale(locale: Locale): void {
    if (locale === this.locale()) return;
    this.locale.set(locale);
    try {
      localStorage.setItem(STORAGE_KEY, locale);
    } catch {
      /* storage unavailable (e.g. private browsing) — locale still applies for this tab */
    }
  }
}
