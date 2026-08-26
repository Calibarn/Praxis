export interface Translation {
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
export const DEFAULT_LOCALE: Locale = 'de';

/** Maps our app locales to BCP-47 tags for Intl date formatting (en = US, fr = Canada). */
export const INTL_LOCALES: Record<Locale, string> = {
  de: 'de-DE',
  en: 'en-US',
  fr: 'fr-CA',
  es: 'es-ES',
};
