import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap, concatMap, map, toArray } from 'rxjs/operators';

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

  private username = 'crupest';
  private organization = 'crupest-web';
  private project = 'Timeline';
  private titleFieldName = 'System.Title';
  private stateFieldName = 'System.State';

  constructor(private client: HttpClient) { }

  private getAzureDevOpsPat(): Observable<string> {
    return this.client.get('/api/TodoList/AzureDevOpsPat', {
      headers: {
        'Accept': 'text/plain'
      },
      responseType: 'text'
    });
  }

  getWorkItemList(): Observable<WorkItem[]> {
    return this.getAzureDevOpsPat().pipe(
      switchMap(
        pat => {
          const headers = new HttpHeaders({
            'Accept': 'application/json',
            'Authorization': `Basic ${btoa(this.username + ':' + pat)}`
          });
          return this.client.post<WiqlResult>(
            `https://dev.azure.com/${this.organization}/${this.project}/_apis/wit/wiql?api-version=5.0`, {
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
