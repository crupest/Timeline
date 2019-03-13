import { Component } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, Event } from '@angular/router';
import { of, Observable } from 'rxjs';
import { delay } from 'rxjs/operators';

import { UserDialogComponent } from './user-dialog.component';

@Component({
  /* tslint:disable-next-line:component-selector*/
  selector: 'router-outlet',
  template: ''
})
class RouterOutletStubComponent { }


describe('UserDialogComponent', () => {
  let component: UserDialogComponent;
  let fixture: ComponentFixture<UserDialogComponent>;


  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [UserDialogComponent, RouterOutletStubComponent],
      providers: [{ // for the workaround
        provide: Router, useValue: {
          events: new Observable<Event>()
        }
      }]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserDialogComponent);
    component = fixture.componentInstance;
  });

});
