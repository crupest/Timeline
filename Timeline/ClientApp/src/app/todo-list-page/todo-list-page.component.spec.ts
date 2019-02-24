import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

import { Observable, from } from 'rxjs';

import { TodoListPageComponent } from './todo-list-page.component';
import { TodoListService, WorkItem } from './todo-list.service';
import { By } from '@angular/platform-browser';
import { delay } from 'rxjs/operators';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

@Component({
  /* tslint:disable-next-line:component-selector*/
  selector: 'mat-progress-bar',
  template: ''
})
class MatProgressBarStubComponent {}

function asyncArray<T>(data: T[]): Observable<T> {
  return from(data).pipe(delay(0));
}

describe('TodoListPageComponent', () => {
  let component: TodoListPageComponent;
  let fixture: ComponentFixture<TodoListPageComponent>;

  let mockWorkItems: WorkItem[];

  beforeEach(async(() => {
    const todoListService: jasmine.SpyObj<TodoListService> = jasmine.createSpyObj('TodoListService', ['getWorkItemList']);

    mockWorkItems = [
      {
        id: 0,
        title: 'Test title 1',
        isCompleted: true,
        detailUrl: 'https://test.org/workitems/0',
        iconUrl: 'https://test.org/icon/0'
      },
      {
        id: 1,
        title: 'Test title 2',
        isCompleted: false,
        detailUrl: 'https://test.org/workitems/1',
        iconUrl: 'https://test.org/icon/1'
      }
    ];

    todoListService.getWorkItemList.and.returnValue(asyncArray(mockWorkItems));

    TestBed.configureTestingModule({
      declarations: [TodoListPageComponent, MatProgressBarStubComponent],
      imports: [NoopAnimationsModule],
      providers: [{ provide: TodoListService, useValue: todoListService }],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TodoListPageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should show progress bar during loading', () => {
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('mat-progress-bar'))).toBeTruthy();
  });

  it('should hide progress bar after loading', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('mat-progress-bar'))).toBeFalsy();
  }));
});
