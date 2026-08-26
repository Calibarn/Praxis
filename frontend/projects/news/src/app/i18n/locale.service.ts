import { DestroyRef, Injectable, inject, signal } from '@angular/core';

import { DEFAULT_LOCALE, Locale, SUPPORTED_LOCALES, Translation } from './translation.model';
import { TRANSLATIONS } from './translations';

const STORAGE_KEY = 'praxis.locale';
const CHANGE_EVENT = 'praxis:locale-change';

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

/**
 * Reads/writes the active locale via localStorage and a window CustomEvent
 * so News stays in sync with the Shell's language switcher even though each
 * is an independently-built federation module with its own instance of this
 * service (see Story 1 "no direct dependencies between microfrontends").
 */
@Injectable({ providedIn: 'root' })
export class LocaleService {
  readonly locale = signal<Locale>(readStoredLocale() ?? detectBrowserLocale() ?? DEFAULT_LOCALE);

  constructor() {
    const listener = (event: Event): void => {
      const locale = (event as CustomEvent<Locale>).detail;
      if (isSupportedLocale(locale)) this.locale.set(locale);
    };
    window.addEventListener(CHANGE_EVENT, listener);
    inject(DestroyRef).onDestroy(() => window.removeEventListener(CHANGE_EVENT, listener));
  }

  translations(): Translation {
    return TRANSLATIONS[this.locale()];
  }
}
