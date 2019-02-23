import { Component, OnInit } from '@angular/core';
import { TodoListService, WorkItem } from './todo-list.service';

@Component({
  selector: 'app-todo-list-page',
  templateUrl: './todo-list-page.component.html',
  styleUrls: ['./todo-list-page.component.css', './todo-list-color-block.css']
})
export class TodoListPageComponent implements OnInit {

  items: WorkItem[];

  constructor(private todoService: TodoListService) {
  }

  ngOnInit() {
    this.todoService.getWorkItemList().subscribe(result => this.items = result);
  }
}
