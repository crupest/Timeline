import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TodoItemComponent } from './todo-item.component';
import { WorkItem } from '../todo-list-page/todo-list.service';
import { By } from '@angular/platform-browser';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('TodoItemComponent', () => {
  let component: TodoItemComponent;
  let fixture: ComponentFixture<TodoItemComponent>;

  let mockWorkItem: WorkItem;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [TodoItemComponent],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  }));

  beforeEach(() => {
    mockWorkItem = {
      id: 0,
      title: 'Title',
      isCompleted: true,
      detailUrl: '/detail',
      iconUrl: '/icon'
    };

    fixture = TestBed.createComponent(TodoItemComponent);
    component = fixture.componentInstance;
    component.item = mockWorkItem;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set icon', () => {
    expect(fixture.debugElement.query(By.css('img.item-icon')).properties['src']).toBe(mockWorkItem.iconUrl);
  });

  it('should set title', () => {
    expect((fixture.debugElement.query(By.css('span.item-title')).nativeElement as HTMLSpanElement).textContent).toBe(
      mockWorkItem.id + '. ' + mockWorkItem.title
    );
  });

  it('should set detail link', () => {
    expect(fixture.debugElement.query(By.css('a.item-detail-button')).properties['href']).toBe(mockWorkItem.detailUrl);
  });
});
