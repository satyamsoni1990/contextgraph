import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { GraphExplorer } from '../../models/graph.models';
import { ContextQueryResponse } from '../../models/context-query.models';

@Injectable({
  providedIn: 'root'
})
export class GraphApiService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5122/api/Graph';

  getProjectGraph(projectId: string): Observable<GraphExplorer> {
    return this.http.get<GraphExplorer>(
      `${this.apiUrl}/explorer/${projectId}`
    );
  }

  queryContext(query: string): Observable<ContextQueryResponse> {
  return this.http.post<ContextQueryResponse>(
    `${this.apiUrl}/query-context`,
    {
      query
    }
  );
}
}