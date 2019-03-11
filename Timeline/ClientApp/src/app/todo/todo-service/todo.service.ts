import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { switchMap, map, filter } from 'rxjs/operators';

import { IssueResponse, githubBaseUrl } from './http-entities';
import { TodoItem } from '../todo-item';


@Injectable({
  providedIn: 'root'
})
export class TodoService {

  constructor(private client: HttpClient) { }

  getWorkItemList(): Observable<TodoItem> {
    return this.client.get<IssueResponse>(`${githubBaseUrl}/issues`, {
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
