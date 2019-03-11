import { Component, Input, OnInit } from '@angular/core';

import { TodoItem } from '../todo-item';
import { throwIfFalsy } from 'src/app/utilities/language-untilities';

@Component({
  selector: 'app-todo-item',
  templateUrl: './todo-item.component.html',
  styleUrls: ['./todo-item.component.css', '../todo-list-color-block.css']
})
export class TodoItemComponent implements OnInit {

  @Input() item: TodoItem | undefined;

  ngOnInit() {
    throwIfFalsy(this.item, 'item');
  }
}
