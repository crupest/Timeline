export const githubBaseUrl = 'https://api.github.com/repos/crupest/Timeline';

export interface IssueResponseItem {
  number: number;
  title: string;
  state: string;
  html_url: string;
  pull_request?: any;
}

export type IssueResponse = IssueResponseItem[];
