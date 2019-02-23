import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

import { defer, Observable } from 'rxjs';

import { TodoListPageComponent } from './todo-list-page.component';
import { TodoListService, WorkItem } from './todo-list.service';
import { By } from '@angular/platform-browser';

@Component({
  /* tslint:disable-next-line:component-selector*/
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

  let mockWorkItems: WorkItem[];

  beforeEach(async(() => {
    const todoListService: jasmine.SpyObj<TodoListService> = jasmine.createSpyObj('TodoListService', ['getWorkItemList']);

    mockWorkItems = [{
      id: 0, title: 'Test title 1', closed: true, detailUrl: 'https://test.org/workitems/0', iconUrl: 'https://test.org/icon/0'
    }, {
      id: 1, title: 'Test title 2', closed: false, detailUrl: 'https://test.org/workitems/1', iconUrl: 'https://test.org/icon/1'
    }];

    todoListService.getWorkItemList.and.returnValue(asyncData(mockWorkItems));

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

  it('should set href on item title', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.debugElement.queryAll(By.css('a.item-title')).forEach((element, index) => {
      expect(element.properties['href']).toBe(mockWorkItems[index].detailUrl);
    });
  }));
});
