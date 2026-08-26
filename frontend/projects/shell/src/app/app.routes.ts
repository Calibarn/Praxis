import { loadRemoteModule } from '@angular-architects/native-federation';
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'news',
    loadComponent: () => loadRemoteModule('news', './Component').then((module) => module.App),
  },
  { path: '', pathMatch: 'full', redirectTo: 'news' },
  { path: '**', redirectTo: 'news' },
];
