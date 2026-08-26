export interface NavItem {
  readonly label: string;
  readonly route: string;
  /** SVG path data (viewBox 0 0 24 24), rendered with fill="none" stroke="currentColor". */
  readonly icon: string;
}

/**
 * Further microfrontends are added here, one entry each; the shell renders
 * the list without further changes (see ADR-0001 / Story 1 criterion 40).
 */
export const NAV_ITEMS: readonly NavItem[] = [
  {
    label: 'News',
    route: '/news',
    icon: 'M6 2h8l4 4v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2Z M14 2v4h4 M7.5 9h4 M7.5 12h9 M7.5 15h9',
  },
];
