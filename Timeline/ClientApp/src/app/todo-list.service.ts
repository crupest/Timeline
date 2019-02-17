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

@Injectable({
  providedIn: 'root'
})
export class TodoListService {

  private username = 'crupest';
  private pat = 'ehnmegogmk6r7qlkpy6zdl2hnfl6ntqbvggzxvvgp4a5vhr7lmnq';
  private organization = 'crupest-web';
  private project = 'Timeline';
  private fieldId = 'System.Title';

  private headers: HttpHeaders;

  constructor(private client: HttpClient) {
    this.headers = new HttpHeaders({
      'Accept': 'application/json',
      'Authorization': `Basic ${btoa(this.username + ':' + this.pat)}`
    });
  }

  getWorkItemList(): Observable<string[]> {
    return this.client.post<WiqlResult>(`https://dev.azure.com/${this.organization}/${this.project}/_apis/wit/wiql?api-version=5.0`, {
      query: 'SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = @project'
    }, { headers: this.headers }).pipe(
      switchMap(result => result.workItems),
      concatMap(result => this.client.get<WorkItemResult>(result.url, {headers: this.headers})),
      map(result => <string>(result.fields[this.fieldId])),
      toArray()
    );
  }
}
