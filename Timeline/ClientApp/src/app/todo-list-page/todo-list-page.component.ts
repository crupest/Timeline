import { Component, OnInit } from '@angular/core';
import { TodoListService, TodoItem } from './todo-list.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-todo-list-page',
  templateUrl: './todo-list-page.component.html',
  styleUrls: ['./todo-list-page.component.css', './todo-list-color-block.css'],
  animations: [
    trigger('itemEnter', [
      transition(':enter', [
        style({
          transform: 'translateX(-100%) translateX(-20px)'
        }),
        animate('400ms ease-out', style({
          transform: 'none'
        }))
      ])
    ])
  ]
})
export class TodoListPageComponent implements OnInit {

  items: TodoItem[] = [];
  isLoadCompleted = false;

  constructor(private todoService: TodoListService) {
  }

  ngOnInit() {
    this.todoService.getWorkItemList().subscribe({
      next: result => this.items.push(result),
      complete: () => { this.isLoadCompleted = true; }
    });
  }
}
