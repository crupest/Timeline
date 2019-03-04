import { Component, ViewChild } from '@angular/core';
import { async, TestBed, ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { DebounceClickDirective } from './debounce-click.directive';

interface TestComponent {
  clickHandler: () => void;
}

@Component({
  selector: 'app-default-test',
  template: '<button (appDebounceClick)="clickHandler()"></button>'
})
class DefaultDebounceTimeTestComponent {
  @ViewChild(DebounceClickDirective)
  directive: DebounceClickDirective;

  clickHandler: () => void = () => { };
}

@Component({
  selector: 'app-default-test',
  template: '<button (appDebounceClick)="clickHandler()" [appDebounceClickTime]="debounceTime"></button>'
})
class CustomDebounceTimeTestComponent {
  debounceTime: number;

  @ViewChild(DebounceClickDirective)
  directive: DebounceClickDirective;

  clickHandler: () => void = () => { };
}


describe('DebounceClickDirective', () => {
  let counter: number;

  function initComponent(component: TestComponent) {
    component.clickHandler = () => counter++;
  }

  beforeEach(() => {
    counter = 0;
  });

  describe('default debounce time', () => {
    let component: DefaultDebounceTimeTestComponent;
    let componentFixture: ComponentFixture<DefaultDebounceTimeTestComponent>;

    beforeEach(async(() => {
      TestBed.configureTestingModule({
        declarations: [DebounceClickDirective, DefaultDebounceTimeTestComponent]
      }).compileComponents();
    }));

    beforeEach(() => {
      componentFixture = TestBed.createComponent(DefaultDebounceTimeTestComponent);
      component = componentFixture.componentInstance;
      initComponent(component);
    });

    it('should create an instance', () => {
      componentFixture.detectChanges();
      expect(component.directive).toBeTruthy();
    });

    it('should work well', fakeAsync(() => {
      function click() {
        (<HTMLButtonElement>componentFixture.debugElement.query(By.css('button')).nativeElement).dispatchEvent(new MouseEvent('click'));
      }
      componentFixture.detectChanges();
      expect(counter).toBe(0);
      click();
      tick(300);
      expect(counter).toBe(0);
      click();
      tick();
      expect(counter).toBe(0);
      tick(500);
      expect(counter).toBe(1);
    }));
  });


  describe('custom debounce time', () => {
    let component: CustomDebounceTimeTestComponent;
    let componentFixture: ComponentFixture<CustomDebounceTimeTestComponent>;

    beforeEach(async(() => {
      TestBed.configureTestingModule({
        declarations: [DebounceClickDirective, CustomDebounceTimeTestComponent]
      }).compileComponents();
    }));

    beforeEach(() => {
      componentFixture = TestBed.createComponent(CustomDebounceTimeTestComponent);
      component = componentFixture.componentInstance;
      initComponent(component);
      component.debounceTime = 600;
    });

    it('should create an instance', () => {
      componentFixture.detectChanges();
      expect(component.directive).toBeTruthy();
    });

    it('should work well', fakeAsync(() => {
      function click() {
        (<HTMLButtonElement>componentFixture.debugElement.query(By.css('button')).nativeElement).dispatchEvent(new MouseEvent('click'));
      }
      componentFixture.detectChanges();
      expect(counter).toBe(0);
      click();
      tick(300);
      expect(counter).toBe(0);
      click();
      tick();
      expect(counter).toBe(0);
      tick(600);
      expect(counter).toBe(1);
    }));
  });
});
