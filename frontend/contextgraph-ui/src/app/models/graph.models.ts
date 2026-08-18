export interface GraphNode {
  id: string;
  label: string;
  displayName: string;
}

export interface GraphRelationship {
  source: string;
  target: string;
  type: string;
}

export interface GraphExplorer {
  nodes: GraphNode[];
  relationships: GraphRelationship[];
}