export interface Translation {
  readonly navToggle: string;
  readonly navClose: string;
  readonly tagline: string;
  readonly navNews: string;
  readonly languageLabel: string;
  readonly heroTitle: string;
  readonly heroLede: string;
  readonly hoursMonFri: string;
  readonly hoursMonThu: string;
  readonly hoursTue: string;
  readonly newsHeading: string;
  readonly emptyState: string;
  readonly loading: string;
  readonly errorMessage: string;
  readonly retry: string;
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

/** Maps our app locales to BCP-47 tags for Intl date formatting (en = US, fr = Canada). */
export const INTL_LOCALES: Record<Locale, string> = {
  de: 'de-DE',
  en: 'en-US',
  fr: 'fr-CA',
  es: 'es-ES',
};
