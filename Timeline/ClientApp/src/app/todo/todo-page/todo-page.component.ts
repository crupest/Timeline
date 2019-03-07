import { Component, OnInit } from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';


import { TodoItem } from '../todo-item';
import { TodoService } from '../todo-service/todo.service';

@Component({
  selector: 'app-todo-page',
  templateUrl: './todo-page.component.html',
  styleUrls: ['./todo-page.component.css', '../todo-list-color-block.css'],
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
export class TodoPageComponent implements OnInit {

  items: TodoItem[] = [];
  isLoadCompleted = false;

  constructor(private todoService: TodoService) {
  }

  ngOnInit() {
    this.todoService.getWorkItemList().subscribe({
      next: result => this.items.push(result),
      complete: () => { this.isLoadCompleted = true; }
    });
  }
}
