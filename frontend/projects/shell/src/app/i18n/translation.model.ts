export interface Translation {
  readonly navToggle: string;
  readonly navClose: string;
  readonly tagline: string;
  readonly navNews: string;
  readonly languageLabel: string;
}

export type Locale = 'de' | 'en' | 'fr' | 'es';

export const SUPPORTED_LOCALES: readonly Locale[] = ['de', 'en', 'fr', 'es'];
export const DEFAULT_LOCALE: Locale = 'en';

/** Each language's own name for itself — conventionally shown unchanged
 * regardless of the current UI locale (en = US, fr = Canada, per request). */
export const LOCALE_NAMES: Record<Locale, string> = {
  de: 'Deutsch',
  en: 'English (US)',
  fr: 'Français (Canada)',
  es: 'Español',
};
