export interface ContextQueryResponse {

  query: string;

  intent: string;

  person: ContextPerson | null;

  connections: ContextConnection[];

}


export interface ContextPerson {

  id: string;

  name: string;

}


export interface ContextConnection {

  relationship: string;

  nodeId: string;

  nodeName: string;

  nodeType: string;

}