import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DebounceClickDirective } from './debounce-click.directive';

@NgModule({
  declarations: [DebounceClickDirective],
  imports: [CommonModule],
  exports: [DebounceClickDirective]
})
export class UtilityModule { }
