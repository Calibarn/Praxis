import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'news',
    loadComponent: () => import('./news/news-feed').then((module) => module.NewsFeedComponent),
  },
  { path: '', pathMatch: 'full', redirectTo: 'news' },
  { path: '**', redirectTo: 'news' },
];
