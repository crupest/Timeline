import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { TodoListService, WorkItem, AzureDevOpsAccessInfo, WiqlResult, WiqlWorkItemResult, WorkItemResult } from './todo-list.service';


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
    expect(service).toBeTruthy();

    const mockWorkItems: WorkItem[] = [{
      id: 0,
      title: 'Test work item 1',
      closed: true
    }, {
      id: 1,
      title: 'Test work item 2',
      closed: false
    }];

    service.getWorkItemList().subscribe(data => {
      expect(data).toEqual(mockWorkItems);
    });

    const httpController: HttpTestingController = TestBed.get(HttpTestingController);

    const mockAccessInfo: AzureDevOpsAccessInfo = {
      username: 'testusername',
      personalAccessToken: 'testtoken',
      organization: 'testorganization',
      project: 'testproject'
    };

    httpController.expectOne('/api/TodoPage/AzureDevOpsAccessInfo').flush(mockAccessInfo);

    const mockWiqlWorkItems: WiqlWorkItemResult[] = [{
      id: 0,
      url: `https://dev.azure.com/${mockAccessInfo.organization}/${mockAccessInfo.project}/_apis/wit/workItems/0`
    }, {
      id: 1,
      url: `https://dev.azure.com/${mockAccessInfo.organization}/${mockAccessInfo.project}/_apis/wit/workItems/1`
    }];

    const authorizationHeader = 'Basic ' + btoa(mockAccessInfo.username + ':' + mockAccessInfo.personalAccessToken);

    httpController.expectOne(req =>
      req.url === `https://dev.azure.com/${mockAccessInfo.organization}/${mockAccessInfo.project}/_apis/wit/wiql?api-version=5.0` &&
      req.headers.get('Authorization') === authorizationHeader
    ).flush(<WiqlResult>{ workItems: mockWiqlWorkItems });

    function mapWorkItemToResult(workItem: WorkItem): WorkItemResult {
      return {
        id: workItem.id,
        fields: {
          [TodoListService.titleFieldName]: workItem.title,
          [TodoListService.stateFieldName]: (workItem.closed ? 'Closed' : 'Active')
        }
      };
    }

    for (let i = 0; i < mockWorkItems.length; i++) {
      httpController.expectOne(req =>
        req.url === mockWiqlWorkItems[i].url &&
        req.headers.get('Authorization') === authorizationHeader
      ).flush(mapWorkItemToResult(mockWorkItems[i]));
    }
  });
});
