import { Component, Input } from '@angular/core';
import { TodoItem } from '../todo-list-page/todo-list.service';

@Component({
  selector: 'app-todo-item',
  templateUrl: './todo-item.component.html',
  styleUrls: ['./todo-item.component.css', '../todo-list-page/todo-list-color-block.css']
})
export class TodoItemComponent {

  @Input() item: TodoItem;

}
