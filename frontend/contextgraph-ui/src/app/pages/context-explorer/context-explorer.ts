import {
  Component,
  OnInit,
  AfterViewInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { FormsModule } from '@angular/forms';


import cytoscape from 'cytoscape';
import { GraphApiService } from '../../core/services/graph-api.service';
import { GraphExplorer } from '../../models/graph.models';
import { ContextQueryResponse } from '../../models/context-query.models';


@Component({
  selector: 'app-context-explorer',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './context-explorer.html',
  styleUrl: './context-explorer.css'
})
export class ContextExplorer
  implements OnInit, AfterViewInit {

  private readonly graphApi = inject(GraphApiService);

  private readonly cdr = inject(ChangeDetectorRef);

  projectId = 'PR001';

  graph: GraphExplorer = {
    nodes: [],
    relationships: []
  };

  loading = false;

  error = '';

  selectedNodeId: string | null = null;

  selectedNodeName = '';

  selectedNodeType = '';
  contextQuery = '';

  contextLoading = false;

  contextError = '';

  contextResult: ContextQueryResponse | null = null;
    // ==========================================
  // AI Context
  // ==========================================

  aiQuestion = '';

  aiLoading = false;

  aiError = '';

  aiAnswer = '';
  selectedNodeRelationships: {
    type: string;
    direction: string;
    otherNodeId: string;
    otherNodeName: string;
  }[] = [];

  private graphInitialized = false;

  private cy: cytoscape.Core | null = null;


  // ==========================================
  // Angular Lifecycle
  // ==========================================

  ngOnInit(): void {
    this.loadGraph();
  }


  ngAfterViewInit(): void {

    this.graphInitialized = true;

    if (this.graph.nodes.length > 0) {

      setTimeout(() => {
        this.renderGraph();
      });

    }
  }

askContext(): void {

  if (!this.contextQuery.trim()) {
    return;
  }

  this.contextLoading = true;

  this.contextError = '';

  this.contextResult = null;

  this.cdr.markForCheck();


  this.graphApi
    .queryContext(this.contextQuery)
    .subscribe({

      next: (result) => {

        console.log(
          'Context query result:',
          result
        );

        this.contextResult = result;

        this.contextLoading = false;

        this.cdr.markForCheck();

      },

      error: (error) => {

        console.error(
          'Context query error:',
          error
        );

        this.contextLoading = false;

        this.contextError =
          'Unable to retrieve work context.';

        this.cdr.markForCheck();

      }

    });
}
  // ==========================================
  // Ask AI using Graph Context
  // ==========================================

  askAI(): void {

    if (!this.aiQuestion.trim()) {
      return;
    }

    this.aiLoading = true;

    this.aiError = '';

    this.aiAnswer = '';

    this.cdr.markForCheck();

    this.graphApi
      .askAIContext(this.aiQuestion)
      .subscribe({

        next: (result) => {

          console.log(
            'AI context result:',
            result
          );

          this.aiAnswer =
            result.answer;

          this.aiLoading = false;

          this.cdr.markForCheck();
        },

        error: (error) => {

          console.error(
            'AI context error:',
            error
          );

          this.aiLoading = false;

          this.aiError =
            'Unable to get an AI answer. Please try again.';

          this.cdr.markForCheck();
        }

      });
  }
  // ==========================================
  // Load Graph
  // ==========================================

  loadGraph(): void {

    this.loading = true;

    this.error = '';

    this.clearSelection();

    this.cdr.markForCheck();

    this.graphApi
      .getProjectGraph(this.projectId)
      .subscribe({

        next: (result) => {

          console.log('Graph loaded:', result);

          this.graph = result;

          this.loading = false;

          console.log(
            'Loading:',
            this.loading
          );

          console.log(
            'Nodes:',
            this.graph.nodes.length
          );

          console.log(
            'Relationships:',
            this.graph.relationships.length
          );

          this.cdr.markForCheck();

          setTimeout(() => {

            this.renderGraph();

            this.cdr.markForCheck();

          });

        },

        error: (error) => {

          console.error(
            'Graph API error:',
            error
          );

          this.loading = false;

          this.error =
            'Unable to load project context from the API.';

          this.cdr.markForCheck();
        },

        complete: () => {

          console.log(
            'Graph API request completed.'
          );

        }
      });
  }


  // ==========================================
  // Render Cytoscape Graph
  // ==========================================

  renderGraph(): void {

    const container =
      document.getElementById('graph-container');

    if (!container) {

      console.log(
        'Graph container not found.'
      );

      return;
    }


    // Destroy previous graph
    if (this.cy) {

      this.cy.destroy();

      this.cy = null;
    }


    const elements: any[] = [];


    // ==========================================
    // Nodes
    // ==========================================

    for (const node of this.graph.nodes) {

      elements.push({

        data: {

          id: node.id,

          label: node.displayName,

          type: node.label
        }

      });

    }


    // ==========================================
    // Relationships
    // ==========================================

    for (const relationship of this.graph.relationships) {

      elements.push({

        data: {

          id:
            `${relationship.source}-${relationship.type}-${relationship.target}`,

          source: relationship.source,

          target: relationship.target,

          label: relationship.type
        }

      });

    }


    // ==========================================
    // Create Cytoscape
    // ==========================================

    this.cy = cytoscape({

      container,

      elements,


      // ========================================
      // Styles
      // ========================================

      style: [

        // Default node
        {
          selector: 'node',

          style: {

            'background-color': '#2563eb',

            'label': 'data(label)',

            'color': '#ffffff',

            'text-valign': 'center',

            'text-halign': 'center',

            'font-size': '12px',

            'font-weight': 600,

            'width': '70px',

            'height': '70px',

            'text-wrap': 'wrap',

            'text-max-width': '65px',

            'border-width': 2,

            'border-color': '#ffffff'
          }
        },


        // Default edge
        {
          selector: 'edge',

          style: {

            'width': 2,

            'line-color': '#94a3b8',

            'target-arrow-color': '#94a3b8',

            'target-arrow-shape': 'triangle',

            'curve-style': 'bezier',

            'label': 'data(label)',

            'font-size': '9px',

            'font-weight': 600,

            'color': '#334155',

            'text-background-color': '#ffffff',

            'text-background-opacity': 1,

            'text-background-padding': '3px'
          }
        },


        // ========================================
        // Project
        // ========================================

        {
          selector: 'node[type="Project"]',

          style: {

            'background-color': '#7c3aed',

            'width': '90px',

            'height': '90px',

            'font-size': '14px',

            'font-weight': 'bold'
          }
        },


        // ========================================
        // Person
        // ========================================

        {
          selector: 'node[type="Person"]',

          style: {

            'background-color': '#059669'
          }
        },


        // ========================================
        // Meeting
        // ========================================

        {
          selector: 'node[type="Meeting"]',

          style: {

            'background-color': '#ea580c'
          }
        },


        // ========================================
        // Decision
        // ========================================

        {
          selector: 'node[type="Decision"]',

          style: {

            'background-color': '#ca8a04'
          }
        },


        // ========================================
        // Task
        // ========================================

        {
          selector: 'node[type="Task"]',

          style: {

            'background-color': '#dc2626'
          }
        },


        // ========================================
        // Document
        // ========================================

        {
          selector: 'node[type="Document"]',

          style: {

            'background-color': '#0891b2'
          }
        },


        // ========================================
        // Email
        // ========================================

        {
          selector: 'node[type="Email"]',

          style: {

            'background-color': '#db2777'
          }
        },


        // ========================================
        // Selected Node
        // ========================================

        {
          selector: 'node:selected',

          style: {

            'border-width': 5,

            'border-color': '#111827',

            'overlay-opacity': 0.15,

            'overlay-color': '#111827'
          }
        }

      ],


      // ========================================
      // Layout
      // ========================================

      layout: {

        name: 'cose',

        animate: true,

        padding: 50
      }

    });


    // ==========================================
    // Node Click
    // ==========================================

    this.cy.on(
      'tap',
      'node',
      (event) => {

        const node =
          event.target;

        const nodeId =
          node.data('id');

        this.selectNode(nodeId);

      }
    );

  }


  // ==========================================
  // Select Node
  // ==========================================

  selectNode(nodeId: string): void {

    const nodeData =
      this.graph.nodes.find(
        x => x.id === nodeId
      );


    if (!nodeData) {

      return;
    }


    this.selectedNodeId =
      nodeData.id;

    this.selectedNodeName =
      nodeData.displayName;

    this.selectedNodeType =
      nodeData.label;


    this.selectedNodeRelationships = [];


    // ==========================================
    // Find relationships
    // ==========================================

    for (
      const relationship
      of this.graph.relationships
    ) {


      // Node is source
      if (
        relationship.source === nodeId
      ) {

        const otherNode =
          this.graph.nodes.find(
            x =>
              x.id === relationship.target
          );


        this.selectedNodeRelationships.push({

          type:
            relationship.type,

          direction:
            '→',

          otherNodeId:
            relationship.target,

          otherNodeName:
            otherNode?.displayName
            ?? relationship.target

        });

      }


      // Node is target
      if (
        relationship.target === nodeId
      ) {

        const otherNode =
          this.graph.nodes.find(
            x =>
              x.id === relationship.source
          );


        this.selectedNodeRelationships.push({

          type:
            relationship.type,

          direction:
            '←',

          otherNodeId:
            relationship.source,

          otherNodeName:
            otherNode?.displayName
            ?? relationship.source

        });

      }

    }


    console.log(
      'Selected node:',
      nodeData
    );

    console.log(
      'Selected relationships:',
      this.selectedNodeRelationships
    );


    this.cdr.markForCheck();
  }


  // ==========================================
  // Clear Selection
  // ==========================================

  clearSelection(): void {

    this.selectedNodeId = null;

    this.selectedNodeName = '';

    this.selectedNodeType = '';

    this.selectedNodeRelationships = [];


    if (this.cy) {

      this.cy.nodes().unselect();

    }


    this.cdr.markForCheck();
  }


  // ==========================================
  // Counts
  // ==========================================

  getNodeCount(): number {

    return this.graph.nodes.length;

  }


  getRelationshipCount(): number {

    return this.graph.relationships.length;

  }

}