import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { toArray } from 'rxjs/operators';

import { TodoItem } from '../todo-item';
import { TodoListService, IssueResponse } from './todo-list.service';


describe('TodoListServiceService', () => {
  beforeEach(() => TestBed.configureTestingModule({
    imports: [HttpClientTestingModule]
  }));

  it('should be created', () => {
    const service: TodoListService = TestBed.get(TodoListService);
    expect(service).toBeTruthy();
  });

  it('should work well', () => {
    const service: TodoListService = TestBed.get(TodoListService);

    const baseUrl = service.baseUrl;

    const mockIssueList: IssueResponse = [{
      number: 1,
      title: 'Issue title 1',
      state: 'open',
      html_url: 'test_url1'
    }, {
      number: 2,
      title: 'Issue title 2',
      state: 'closed',
      html_url: 'test_url2',
      pull_request: {}
    }];

    const mockTodoItemList: TodoItem[] = [{
      number: 1,
      title: 'Issue title 1',
      isClosed: false,
      detailUrl: 'test_url1'
    }];

    service.getWorkItemList().pipe(toArray()).subscribe(data => {
      expect(data).toEqual(mockTodoItemList);
    });

    const httpController: HttpTestingController = TestBed.get(HttpTestingController);

    httpController.expectOne(request => request.url === baseUrl + '/issues' && request.params.get('state') === 'all').flush(mockIssueList);

    httpController.verify();
  });
});
