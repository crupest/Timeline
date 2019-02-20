import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap, concatMap, map, toArray } from 'rxjs/operators';

interface AzureDevOpsAccessInfo {
  username: string;
  personalAccessToken: string;
  organization: string;
  project: string;
}

interface WiqlWorkItemResult {
  id: number;
  url: string;
}

interface WiqlResult {
  workItems: WiqlWorkItemResult[];
}

interface WorkItemResult {
  id: number;
  fields: { [name: string]: any };
}


export interface WorkItem {
  id: number;
  title: string;
  closed: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TodoListService {

  private titleFieldName = 'System.Title';
  private stateFieldName = 'System.State';

  constructor(private client: HttpClient) { }

  private getAzureDevOpsPat(): Observable<AzureDevOpsAccessInfo> {
    return this.client.get<AzureDevOpsAccessInfo>('/api/TodoPage/AzureDevOpsAccessInfo');
  }

  getWorkItemList(): Observable<WorkItem[]> {
    return this.getAzureDevOpsPat().pipe(
      switchMap(
        accessInfo => {
          const headers = new HttpHeaders({
            'Accept': 'application/json',
            'Authorization': `Basic ${btoa(accessInfo.username + ':' + accessInfo.personalAccessToken)}`
          });
          return this.client.post<WiqlResult>(
            `https://dev.azure.com/${accessInfo.organization}/${accessInfo.project}/_apis/wit/wiql?api-version=5.0`, {
              query: 'SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = @project'
            }, { headers: headers }).pipe(
              switchMap(result => result.workItems),
              concatMap(result => this.client.get<WorkItemResult>(result.url, { headers: headers })),
              map(result => <WorkItem>{
                id: result.id,
                title: <string>result.fields[this.titleFieldName],
                closed: ((<string>result.fields[this.stateFieldName]).toLowerCase() === 'closed')
              }),
              toArray()
            );
        }
      )
    );
  }
}
