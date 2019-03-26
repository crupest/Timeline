import { Directive, Input } from '@angular/core';

@Directive({
  /* tslint:disable-next-line:directive-selector*/
  selector: '[routerLink]'
})
export class RouterLinkStubDirective {
  @Input('routerLink') linkParams: any;
}
