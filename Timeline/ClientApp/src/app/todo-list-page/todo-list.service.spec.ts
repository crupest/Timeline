import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import {
  TodoListService, WorkItem, AzureDevOpsAccessInfo,
  WiqlResult, WiqlWorkItemResult, WorkItemResult, WorkItemTypeResult
} from './todo-list.service';
import { toArray } from 'rxjs/operators';


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

    const mockAccessInfo: AzureDevOpsAccessInfo = {
      username: 'testusername',
      personalAccessToken: 'testtoken',
      organization: 'testorganization',
      project: 'testproject'
    };

    const baseUrl = `https://dev.azure.com/${mockAccessInfo.organization}/${mockAccessInfo.project}/`;

    const mockWorkItems: WorkItem[] = Array.from({ length: 2 }, (_, i) => <WorkItem>{
      id: i,
      title: 'Test work item ' + i,
      isCompleted: i === 0,
      detailUrl: `${baseUrl}_workitems/edit/${i}/`,
      iconUrl: `${baseUrl}_api/wit/icon/${i}`,
    });

    const workItemTypeMap = new Map<WorkItem, string>(Array.from(mockWorkItems, v => <[WorkItem, string]>[v, 'type' + v.id]));

    service.getWorkItemList().pipe(toArray()).subscribe(data => {
      expect(data).toEqual(mockWorkItems);
    });

    const httpController: HttpTestingController = TestBed.get(HttpTestingController);

    httpController.expectOne('/api/TodoPage/AzureDevOpsAccessInfo').flush(mockAccessInfo);

    const mockWiqlWorkItems: WiqlWorkItemResult[] = Array.from(mockWorkItems, v => <WiqlWorkItemResult>{
      id: v.id,
      url: `${baseUrl}_apis/wit/workItems/${v.id}`
    });

    const authorizationHeader = 'Basic ' + btoa(mockAccessInfo.username + ':' + mockAccessInfo.personalAccessToken);

    httpController.expectOne(req =>
      req.url === `${baseUrl}_apis/wit/wiql?api-version=5.0` &&
      req.headers.get('Authorization') === authorizationHeader
    ).flush(<WiqlResult>{ workItems: mockWiqlWorkItems });

    function mapWorkItemToResult(mockWorkItem: WorkItem): WorkItemResult {
      return {
        id: mockWorkItem.id,
        fields: {
          [TodoListService.titleFieldName]: mockWorkItem.title,
          [TodoListService.stateFieldName]: (mockWorkItem.isCompleted ? 'Closed' : 'Active'),
          [TodoListService.typeFieldName]: workItemTypeMap.get(mockWorkItem)
        }
      };
    }

    for (let i = 0; i < mockWorkItems.length; i++) {
      httpController.expectOne(req =>
        req.url === mockWiqlWorkItems[i].url &&
        req.headers.get('Authorization') === authorizationHeader
      ).flush(mapWorkItemToResult(mockWorkItems[i]));

      httpController.expectOne(req =>
        req.url === `${baseUrl}_apis/wit/workitemtypes/${encodeURIComponent(workItemTypeMap.get(mockWorkItems[i]))}?api-version=5.0` &&
        req.headers.get('Authorization') === authorizationHeader
      ).flush(<WorkItemTypeResult>{
        icon: {
          url: mockWorkItems[i].iconUrl
        }
      });
    }

    httpController.verify();
  });
});
