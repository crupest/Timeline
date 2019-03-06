import { Directive, Output, Input, EventEmitter, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { fromEvent, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Directive({
  selector: '[appDebounceClick]'
})
export class DebounceClickDirective implements OnInit, OnDestroy {

  private subscription: Subscription;

  @Output('appDebounceClick') clickEvent = new EventEmitter<any>();

  // tslint:disable-next-line:no-input-rename
  @Input('appDebounceClickTime')
  set debounceTime(value: number) {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
    this.subscription = fromEvent(<HTMLElement>this.element.nativeElement, 'click').pipe(
      debounceTime(value)
    ).subscribe(o => this.clickEvent.emit(o));
  }

  constructor(private element: ElementRef) {
  }

  ngOnInit() {
    if (!this.subscription) {
      this.subscription = fromEvent(<HTMLElement>this.element.nativeElement, 'click').pipe(
        debounceTime(500)
      ).subscribe(o => this.clickEvent.emit(o));
    }
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }
}
