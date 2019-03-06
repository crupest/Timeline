import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { Observable, from } from 'rxjs';
import { delay } from 'rxjs/operators';

import { TodoItem } from '../todo-item';
import { TodoListPageComponent } from './todo-list-page.component';
import { TodoListService } from '../todo-service/todo-list.service';


@Component({
  /* tslint:disable-next-line:component-selector*/
  selector: 'mat-progress-bar',
  template: ''
})
class MatProgressBarStubComponent { }

function asyncArray<T>(data: T[]): Observable<T> {
  return from(data).pipe(delay(0));
}

describe('TodoListPageComponent', () => {
  let component: TodoListPageComponent;
  let fixture: ComponentFixture<TodoListPageComponent>;

  const mockTodoItems: TodoItem[] = [
    {
      number: 0,
      title: 'Test title 1',
      isClosed: true,
      detailUrl: 'test_url1'
    },
    {
      number: 1,
      title: 'Test title 2',
      isClosed: false,
      detailUrl: 'test_url2'
    }
  ];

  beforeEach(async(() => {
    const todoListService: jasmine.SpyObj<TodoListService> = jasmine.createSpyObj('TodoListService', ['getWorkItemList']);

    todoListService.getWorkItemList.and.returnValue(asyncArray(mockTodoItems));

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
