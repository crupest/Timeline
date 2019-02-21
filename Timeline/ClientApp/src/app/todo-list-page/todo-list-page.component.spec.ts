import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed, tick } from '@angular/core/testing';

import { defer, Observable } from 'rxjs';

import { TodoListPageComponent } from './todo-list-page.component';
import { TodoListService, WorkItem } from './todo-list.service';
import { By } from '@angular/platform-browser';

@Component({
  selector: 'mat-progress-bar',
  template: ''
})
class MatProgressBarStubComponent {

}

function asyncData<T>(data: T): Observable<T> {
  return defer(() => Promise.resolve(data));
}

describe('TodoListPageComponent', () => {
  let component: TodoListPageComponent;
  let fixture: ComponentFixture<TodoListPageComponent>;

  beforeEach(async(() => {
    const todoListService: jasmine.SpyObj<TodoListService> = jasmine.createSpyObj('TodoListService', ['getWorkItemList']);

    todoListService.getWorkItemList.and.returnValue(asyncData(<WorkItem[]>[{
      id: 0, title: 'Test title 1', closed: true
    }, {
      id: 1, title: 'Test title 2', closed: false
    }]));

    TestBed.configureTestingModule({
      declarations: [TodoListPageComponent, MatProgressBarStubComponent],
      providers: [
        { provide: TodoListService, useValue: todoListService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TodoListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show progress bar during loading', () => {
    expect(fixture.debugElement.query(By.css('mat-progress-bar'))).toBeTruthy();
  });

  it('should hide progress bar after loading', async(() => {
    fixture.whenStable().then(() => {
      fixture.detectChanges();
      expect(fixture.debugElement.query(By.css('mat-progress-bar'))).toBeFalsy();
    });
  }));
});
