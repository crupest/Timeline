import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { Observable, from } from 'rxjs';
import { delay } from 'rxjs/operators';

import { TodoItem } from '../todo-item';
import { TodoPageComponent } from './todo-page.component';
import { TodoService } from '../todo-service/todo.service';


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
  let component: TodoPageComponent;
  let fixture: ComponentFixture<TodoPageComponent>;

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
    const mockTodoService: jasmine.SpyObj<TodoService> = jasmine.createSpyObj('TodoService', ['getWorkItemList']);

    mockTodoService.getWorkItemList.and.returnValue(asyncArray(mockTodoItems));

    TestBed.configureTestingModule({
      declarations: [TodoPageComponent, MatProgressBarStubComponent],
      imports: [NoopAnimationsModule],
      providers: [{ provide: TodoService, useValue: mockTodoService }],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TodoPageComponent);
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
