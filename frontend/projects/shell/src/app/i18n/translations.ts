import { Locale, Translation } from './translation.model';
import { de } from './de';
import { en } from './en';
import { es } from './es';
import { fr } from './fr';

export const TRANSLATIONS: Record<Locale, Translation> = { de, en, es, fr };
