import { Component } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, Event } from '@angular/router';

import { Observable } from 'rxjs';

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
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
