import { NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { TodoItem } from '../todo-item';
import { TodoItemComponent } from '../todo-item/todo-item.component';

describe('TodoItemComponent', () => {
  let component: TodoItemComponent;
  let fixture: ComponentFixture<TodoItemComponent>;

  const mockTodoItem: TodoItem = {
    number: 1,
    title: 'Title',
    isClosed: true,
    detailUrl: '/detail',
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [TodoItemComponent],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TodoItemComponent);
    component = fixture.componentInstance;
    component.item = mockTodoItem;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set title', () => {
    expect((fixture.debugElement.query(By.css('span.item-title')).nativeElement as HTMLSpanElement).textContent).toBe(
      mockTodoItem.number + '. ' + mockTodoItem.title
    );
  });

  it('should set detail link', () => {
    expect(fixture.debugElement.query(By.css('a.item-detail-button')).properties['href']).toBe(mockTodoItem.detailUrl);
  });
});
