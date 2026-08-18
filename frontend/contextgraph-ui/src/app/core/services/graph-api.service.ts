import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { GraphExplorer } from '../../models/graph.models';
import { ContextQueryResponse } from '../../models/context-query.models';

export interface AIContextResponse {
  question: string;
  answer: string;
}

@Injectable({
  providedIn: 'root'
})
export class GraphApiService {

  private readonly http = inject(HttpClient);

  private readonly graphApiUrl =
    'http://localhost:5122/api/Graph';

  private readonly aiApiUrl =
    'http://localhost:5122/api/AI';


  getProjectGraph(
    projectId: string
  ): Observable<GraphExplorer> {

    return this.http.get<GraphExplorer>(
      `${this.graphApiUrl}/explorer/${projectId}`
    );
  }


  queryContext(
    query: string
  ): Observable<ContextQueryResponse> {

    return this.http.post<ContextQueryResponse>(
      `${this.graphApiUrl}/query-context`,
      {
        query
      }
    );
  }


  askAIContext(
    question: string
  ): Observable<AIContextResponse> {

    return this.http.post<AIContextResponse>(
      `${this.aiApiUrl}/context`,
      {
        question
      }
    );
  }
}