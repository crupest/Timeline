import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { switchMap, map, filter } from 'rxjs/operators';

import { TodoItem } from '../todo-item';

export interface IssueResponseItem {
  number: number;
  title: string;
  state: string;
  html_url: string;
  pull_request?: any;
}

export type IssueResponse = IssueResponseItem[];

@Injectable({
  providedIn: 'root'
})
export class TodoService {

  readonly baseUrl = 'https://api.github.com/repos/crupest/Timeline';

  constructor(private client: HttpClient) { }

  getWorkItemList(): Observable<TodoItem> {
    return this.client.get<IssueResponse>(`${this.baseUrl}/issues`, {
      params: {
        state: 'all'
      }
    }).pipe(
      switchMap(result => from(result)),
      filter(result => result.pull_request === undefined), // filter out pull requests.
      map(result => <TodoItem>{
        number: result.number,
        title: result.title,
        isClosed: result.state === 'closed',
        detailUrl: result.html_url
      })
    );
  }
}
