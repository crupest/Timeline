import { ParamMap } from '@angular/router';

interface MockActivatedRoute {
  snapshot: MockActivatedRouteSnapshot;
}

interface MockActivatedRouteSnapshot {
  paramMap: ParamMap;
}

export function createMockActivatedRoute(mockParamMap: { [name: string]: string | string[] }): MockActivatedRoute {
  return {
    snapshot: {
      paramMap: {
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
      }
    }
  }
}
