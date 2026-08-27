import { Pipe, PipeTransform, inject } from '@angular/core';

import { LocaleService } from './locale.service';
import { INTL_LOCALES } from './translation.model';

@Pipe({ name: 'localeDate', pure: false })
export class LocaleDatePipe implements PipeTransform {
  private readonly localeService = inject(LocaleService);

  transform(value: string): string {
    const intlLocale = INTL_LOCALES[this.localeService.locale()];
    return new Intl.DateTimeFormat(intlLocale, { dateStyle: 'medium', timeStyle: 'short' }).format(
      new Date(value),
    );
  }
}
