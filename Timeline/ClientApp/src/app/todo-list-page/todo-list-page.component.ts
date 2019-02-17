import { Component, OnInit } from '@angular/core';
import { TodoListService } from '../todo-list.service';

@Component({
  selector: 'app-todo-list-page',
  templateUrl: './todo-list-page.component.html',
  styleUrls: ['./todo-list-page.component.css']
})
export class TodoListPageComponent implements OnInit {

  items: string[];

  constructor(private todoService: TodoListService) {
    todoService.getWorkItemList().subscribe(result => this.items = result);
  }

  ngOnInit() {
  }

}
