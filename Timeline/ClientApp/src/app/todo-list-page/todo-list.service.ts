import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap, concatMap, map, toArray } from 'rxjs/operators';

export interface AzureDevOpsAccessInfo {
  username: string;
  personalAccessToken: string;
  organization: string;
  project: string;
}

export interface WiqlWorkItemResult {
  id: number;
  url: string;
}

export interface WiqlResult {
  workItems: WiqlWorkItemResult[];
}

export interface WorkItemResult {
  id: number;
  fields: { [name: string]: any };
}


export interface WorkItem {
  id: number;
  title: string;
  closed: boolean;
  detailUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class TodoListService {

  public static titleFieldName = 'System.Title';
  public static stateFieldName = 'System.State';

  constructor(private client: HttpClient) { }

  private getAzureDevOpsAccessInfo(): Observable<AzureDevOpsAccessInfo> {
    return this.client.get<AzureDevOpsAccessInfo>('/api/TodoPage/AzureDevOpsAccessInfo');
  }

  getWorkItemList(): Observable<WorkItem[]> {
    return this.getAzureDevOpsAccessInfo().pipe(
      switchMap(
        accessInfo => {
          const baseUrl = `https://dev.azure.com/${accessInfo.organization}/${accessInfo.project}/`;
          const headers = new HttpHeaders({
            'Accept': 'application/json',
            'Authorization': `Basic ${btoa(accessInfo.username + ':' + accessInfo.personalAccessToken)}`
          });
          return this.client.post<WiqlResult>(
            `${baseUrl}_apis/wit/wiql?api-version=5.0`, {
              query: 'SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = @project'
            }, { headers: headers }).pipe(
              switchMap(result => result.workItems),
              concatMap(result => this.client.get<WorkItemResult>(result.url, { headers: headers })),
              map(result => <WorkItem>{
                id: result.id,
                title: <string>result.fields[TodoListService.titleFieldName],
                closed: ((<string>result.fields[TodoListService.stateFieldName]).toLowerCase() === 'closed'),
                detailUrl: `${baseUrl}_workitems/edit/${result.id}/`
              }),
              toArray()
            );
        }
      )
    );
  }
}
