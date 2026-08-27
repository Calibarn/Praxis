import { Translation } from './i18n/translation.model';

export interface NavItem {
  readonly labelKey: keyof Translation;
  readonly route: string;
  /** SVG path data (viewBox 0 0 24 24), rendered with fill="none" stroke="currentColor". */
  readonly icon: string;
}

/** Further pages/sections are added here, one entry each; the shell chrome
 * renders the list without further changes. */
export const NAV_ITEMS: readonly NavItem[] = [
  {
    labelKey: 'navNews',
    route: '/news',
    icon: 'M6 2h8l4 4v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2Z M14 2v4h4 M7.5 9h4 M7.5 12h9 M7.5 15h9',
  },
];
