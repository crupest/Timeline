import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule, MatIconModule, MatCardModule, MatProgressBarModule, MatButtonModule } from '@angular/material';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { TodoItemComponent } from './todo-item/todo-item.component';
import { TodoPageComponent } from './todo-page/todo-page.component';

@NgModule({
  declarations: [
    TodoItemComponent,
    TodoPageComponent
  ],
  imports: [
    CommonModule, HttpClientModule, BrowserAnimationsModule,
    MatListModule, MatCardModule, MatIconModule, MatProgressBarModule, MatButtonModule,
    RouterModule.forChild([
      { path: 'todo', component: TodoPageComponent }
    ])
  ],
  exports: [
    RouterModule
  ]
})
export class TodoModule { }
