import { ParamMap, ActivatedRouteSnapshot, ActivatedRoute } from '@angular/router';

import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

import { PartialMock } from './mock';

export interface ParamMapCreator { [name: string]: string | string[]; }

export class MockActivatedRouteSnapshot implements PartialMock<ActivatedRouteSnapshot> {

  private paramMapInternal: ParamMap;

  constructor({ mockParamMap }: { mockParamMap: ParamMapCreator } = { mockParamMap: {} }) {
    this.paramMapInternal = {
      keys: Object.keys(mockParamMap),
      get(name: string): string | null {
        const param = mockParamMap[name];
        if (typeof param === 'string') {
          return param;
        } else if (param instanceof Array) {
          if (param.length === 0) {
            return null;
          }
          return param[0];
        }
        return null;
      },
      getAll(name: string): string[] {
        const param = mockParamMap[name];
        if (typeof param === 'string') {
          return [param];
        } else if (param instanceof Array) {
          return param;
        }
        return [];
      },
      has(name: string): boolean {
        return mockParamMap.hasOwnProperty(name);
      }
    };
  }

  get paramMap(): ParamMap {
    return this.paramMapInternal;
  }
}

export class MockActivatedRoute implements PartialMock<ActivatedRoute> {

  snapshot$ = new BehaviorSubject<MockActivatedRouteSnapshot>(new MockActivatedRouteSnapshot());

  get paramMap(): Observable<ParamMap> {
    return this.snapshot$.pipe(map(snapshot => snapshot.paramMap));
  }

  get snapshot(): MockActivatedRouteSnapshot {
    return this.snapshot$.value;
  }

  pushSnapshot(snapshot: MockActivatedRouteSnapshot) {
    this.snapshot$.next(snapshot);
  }

  pushSnapshotWithParamMap(mockParamMap: ParamMapCreator) {
    this.pushSnapshot(new MockActivatedRouteSnapshot({mockParamMap}));
  }
}
