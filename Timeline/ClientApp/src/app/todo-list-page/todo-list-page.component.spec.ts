import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TodoListPageComponent } from './todo-list-page.component';

describe('TodoListPageComponent', () => {
  let component: TodoListPageComponent;
  let fixture: ComponentFixture<TodoListPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TodoListPageComponent ]
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
});
