import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { switchMap, concatMap, map } from 'rxjs/operators';

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

export interface WorkItemTypeResult {
  icon: {
    url: string;
  };
}

export interface WorkItem {
  id: number;
  title: string;
  isCompleted: boolean;
  detailUrl: string;
  iconUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class TodoListService {
  public static titleFieldName = 'System.Title';
  public static stateFieldName = 'System.State';
  public static typeFieldName = 'System.WorkItemType';

  constructor(private client: HttpClient) {}

  private getAzureDevOpsAccessInfo(): Observable<AzureDevOpsAccessInfo> {
    return this.client.get<AzureDevOpsAccessInfo>('/api/TodoPage/AzureDevOpsAccessInfo');
  }

  private getItemIconUrl(baseUrl: string, headers: HttpHeaders, type: string): Observable<string> {
    return this.client
      .get<WorkItemTypeResult>(`${baseUrl}_apis/wit/workitemtypes/${encodeURIComponent(type)}?api-version=5.0`, {
        headers: headers
      })
      .pipe(map(result => result.icon.url));
  }

  getWorkItemList(): Observable<WorkItem> {
    return this.getAzureDevOpsAccessInfo().pipe(
      switchMap(accessInfo => {
        const baseUrl = `https://dev.azure.com/${accessInfo.organization}/${accessInfo.project}/`;
        const headers = new HttpHeaders({
          Accept: 'application/json',
          Authorization: `Basic ${btoa(accessInfo.username + ':' + accessInfo.personalAccessToken)}`
        });
        return this.client
          .post<WiqlResult>(
            `${baseUrl}_apis/wit/wiql?api-version=5.0`,
            {
              query: 'SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = @project'
            },
            { headers: headers }
          )
          .pipe(
            concatMap(result => from(result.workItems)),
            concatMap(result => this.client.get<WorkItemResult>(result.url, { headers: headers })),
            concatMap(result =>
              this.getItemIconUrl(baseUrl, headers, result.fields[TodoListService.typeFieldName]).pipe(
                map(
                  iconResult =>
                    <WorkItem>{
                      id: result.id,
                      title: <string>result.fields[TodoListService.titleFieldName],
                      isCompleted: (function(stateErasedCase: string): Boolean {
                        return stateErasedCase === 'closed' || stateErasedCase === 'resolved';
                      })((result.fields[TodoListService.stateFieldName] as string).toLowerCase()),
                      detailUrl: `${baseUrl}_workitems/edit/${result.id}/`,
                      iconUrl: iconResult
                    }
                )
              )
            )
          );
      })
    );
  }
}
