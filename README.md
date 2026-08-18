
# ContextGraph

**ContextGraph** is an AI-ready Work Context Explorer that uses a graph database to connect people, projects, meetings, decisions, tasks, documents, and emails into a single connected context.

The current implementation provides an interactive graph explorer and context search API. The next planned phase is to add an LLM/Graph-RAG layer for natural-language answers.

---

## What is ContextGraph?

Traditional business applications often store work information in separate systems:

```text
People
Projects
Tasks
Meetings
Documents
Emails
```

ContextGraph connects these entities through relationships.

For example:

```text
                    Phoenix
                       |
          +------------+------------+
          |            |            |
       WORKS_ON     ATTENDED     AUTHORED
          |            |            |
        Satyam      Meeting      Document
          |
     ASSIGNED_TO
          |
         Task
```

This allows the system to answer questions using connected work context rather than isolated records.

---

# Key Use Cases

ContextGraph is designed for work-context intelligence.

Examples:

### What is Satyam working on?

```text
Satyam
   |
   +-- WORKS_ON --> Phoenix
   |
   +-- ASSIGNED_TO --> Prepare release plan
   |
   +-- ATTENDED --> Phoenix Sprint Planning
   |
   +-- AUTHORED --> Phoenix Release Plan
   |
   +-- SENT --> Phoenix Release Timeline
```

### Other business questions

- What is a person currently working on?
- What tasks are assigned to a person?
- Who is working on a project?
- What happened in a meeting?
- What decisions were made?
- Which documents are related to a project?
- Which emails are related to a work item?
- What context should a user know before taking an action?

---

# Technology Stack

## Backend

- C#
- ASP.NET Core
- REST API
- Neo4j-compatible .NET Driver

## Database

- CognoDB
- Graph database
- Cypher

## Frontend

- Angular
- TypeScript
- Cytoscape.js
- HTML
- CSS

## Development

- Visual Studio
- Node.js / npm
- Git / GitHub

---

# Architecture

Current architecture:

```text
                         User
                          |
                          v
                 Angular Application
                          |
             +------------+------------+
             |                         |
             v                         v
      Context Search             Graph Explorer
             |                         |
             +------------+------------+
                          |
                          v
                   ASP.NET Core API
                          |
             +------------+------------+
             |                         |
             v                         v
       Context Service          Graph Repository
             |                         |
             +------------+------------+
                          |
                          v
                        CognoDB
                          |
                          v
                    Graph Context
```

---

# Current Data Model

The current graph contains:

```text
Person
Project
Meeting
Decision
Task
Document
Email
```

Example relationships include:

```text
WORKS_ON
ATTENDED
ASSIGNED_TO
AUTHORED
SENT
```

Example:

```text
Satyam
   |
   +-- WORKS_ON ------> Phoenix
   |
   +-- ATTENDED -------> Phoenix Sprint Planning
   |
   +-- ASSIGNED_TO ----> Prepare release plan
   |
   +-- AUTHORED -------> Phoenix Release Plan
   |
   +-- SENT ------------> Phoenix Release Timeline
```

The current working graph contains:

```text
7 nodes
10 relationships
```

---

# Project Structure

```text
contextgraph/
│
├── .gitignore
├── README.md
│
├── backend/
│   └── ContextGraph.Api/
│       │
│       ├── Controllers/
│       ├── Models/
│       ├── Repositories/
│       ├── Services/
│       ├── Configuration/
│       └── Program.cs
│
├── frontend/
│   └── contextgraph-ui/
│       │
│       ├── src/
│       │   └── app/
│       │       ├── core/
│       │       ├── models/
│       │       └── context-explorer/
│       │
│       └── package.json
│
├── database/
├── docs/
└── scripts/
```

---

# Backend

The ASP.NET Core API is responsible for:

- Connecting to CognoDB
- Executing Cypher queries
- Retrieving graph context
- Detecting query intent
- Returning structured context to Angular

Important areas:

```text
Controllers/
Models/
Repositories/
Services/
Configuration/
Program.cs
```

---

# CognoDB

ContextGraph uses CognoDB as the graph database.

The backend uses the Neo4j-compatible .NET driver to execute Cypher queries.

Example:

```cypher
MATCH (p:Person)-[r:WORKS_ON]->(project:Project)
RETURN p, r, project
```

Graph relationships are the key to retrieving connected work context.

---

# API

## Project Graph

The application provides an endpoint for retrieving project context.

Example:

```http
GET /api/Graph/explorer/PR001
```

The response contains graph nodes and relationships used by the Angular graph explorer.

---

## Context Query

Endpoint:

```http
POST /api/Graph/query-context
```

Request:

```json
{
  "query": "What is Satyam working on?"
}
```

Current processing flow:

```text
Question
   |
   v
ContextQueryService
   |
   v
Intent Detection
   |
   v
Cypher Query
   |
   v
CognoDB
   |
   v
Relevant Context
```

Example response:

```json
{
  "query": "What is Satyam working on?",
  "intent": "WorkingOn",
  "person": {
    "id": "P001",
    "name": "Satyam"
  },
  "connections": [
    {
      "relationship": "WORKS_ON",
      "nodeId": "PR001",
      "nodeName": "Phoenix",
      "nodeType": "Project"
    }
  ]
}
```

---

# Query Intent Detection

The current implementation uses simple rule-based intent detection.

Supported intents:

```text
General
WorkingOn
Tasks
Meeting
Decisions
WhoWorking
```

Examples:

| User Question                             | Intent     |
| ----------------------------------------- | ---------- |
| What is Satyam working on?                | WorkingOn  |
| What tasks are assigned to Satyam?        | Tasks      |
| What happened in Phoenix Sprint Planning? | Meeting    |
| What decisions were made?                 | Decisions  |
| Who is working on Phoenix?                | WhoWorking |

This is intentionally rule-based in the current version.

The planned AI phase will replace or enhance this with LLM-based query understanding.

---

# Frontend

The Angular application provides two main capabilities:

## 1. Graph Explorer

The graph is rendered using Cytoscape.js.

Users can:

- Zoom
- Pan
- Move nodes
- View relationship labels
- Click nodes
- View node details

When a node is selected, the application displays:

```text
Node Details

Name
Type
ID

Connected Context
Relationship
Connected Node
Connected Node ID
```

---

## 2. Context Search

The UI contains a search box:

```text
Ask about your work context

[ What is Satyam working on? ] [ Ask Context ]
```

Angular sends the request to:

```text
POST /api/Graph/query-context
```

The result displays:

- Query
- Intent
- Person
- Connected context
- Relationship
- Node type
- Node ID

---

# Angular FormsModule

The Context Explorer is a standalone Angular component.

`FormsModule` is therefore imported directly into the component:

```typescript
import { FormsModule } from '@angular/forms';
```

And:

```typescript
@Component({
  selector: 'app-context-explorer',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './context-explorer.html',
  styleUrl: './context-explorer.css'
})
```

This enables:

```html
[(ngModel)]
```

---

# Current Angular Models

```typescript
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
```

---

# Running the Project

## Backend

Open the solution in Visual Studio.

Start:

```text
ContextGraph.Api
```

Swagger should be available when the API starts.

---

## Frontend

Open a terminal:

```powershell
cd frontend/contextgraph-ui
```

Install dependencies:

```powershell
npm install
```

Start Angular:

```powershell
ng serve
```

Open:

```text
http://localhost:4200
```

---

# Git and .gitignore

The repository should contain a `.gitignore` at the root.

Recommended:

```gitignore
# Visual Studio
.vs/
*.user
*.suo

# .NET
**/bin/
**/obj/
TestResults/

# Angular / Node
**/node_modules/
**/dist/
**/.angular/

# Environment / secrets
.env
.env.*
!.env.example
**/secrets.json

# Logs
*.log

# OS
.DS_Store
Thumbs.db
```

Never commit:

```text
API keys
Passwords
Connection strings
Secrets
.env files
secrets.json
node_modules
bin
obj
.vs
dist
```

---

# Current Successful Scenario

The following question is currently supported:

```text
What is Satyam working on?
```

The graph/context contains information such as:

```text
Satyam
P001

WORKS_ON
Phoenix
PR001

ASSIGNED_TO
Prepare release plan
T001

ATTENDED
Phoenix Sprint Planning
M001

AUTHORED
Phoenix Release Plan
DOC001

SENT
Phoenix Release Timeline
E001
```

The Angular application successfully displays the retrieved context.

---

# Design Principle

The important architectural principle is:

> Retrieve verified business context from the graph first, then use AI to reason over that context.

The planned architecture is:

```text
User Question
      |
      v
Query Understanding
      |
      v
CognoDB Graph Retrieval
      |
      v
Verified Work Context
      |
      v
LLM
      |
      v
Natural Language Answer
```

The LLM should not invent project information that is not present in the retrieved context.

---

# Future AI / Graph-RAG Architecture

The next major phase is AI integration.

Planned flow:

```text
                    User Question
                          |
                          v
                  Query Understanding
                          |
                          v
                    CognoDB Graph
                          |
                          v
                 Relevant Graph Context
                          |
                          v
                         LLM
                          |
                          v
                Natural Language Answer
```

Example future answer:

```text
Satyam is working on the Phoenix project.
His current assigned task is "Prepare release plan",
which is currently Open. The work was discussed
during the Phoenix Sprint Planning meeting.
```

The LLM will be placed behind an interface so the provider can be changed later without changing the graph/business logic.

Possible future providers include:

```text
OpenRouter
Azure OpenAI
OpenAI
```

---

# Roadmap

## Phase 1 — Foundation

- [X] ASP.NET Core API
- [X] CognoDB connection
- [X] Graph data creation
- [X] Cypher queries
- [X] Project context retrieval
- [X] Angular application
- [X] CORS

## Phase 2 — Graph Explorer

- [X] Cytoscape.js
- [X] Interactive graph
- [X] Node selection
- [X] Node details
- [X] Relationship visualization
- [X] Graph legend

## Phase 3 — Context Search

- [X] Context query API
- [X] Person context retrieval
- [X] Query intent detection
- [X] Task queries
- [X] Meeting queries
- [X] Decision queries
- [X] Who-is-working queries
- [X] Angular context search

## Phase 4 — AI / Graph RAG

- [ ] LLM integration
- [ ] Natural-language query understanding
- [ ] Context-aware answer generation
- [ ] Graph RAG pipeline
- [ ] Source/context references
- [ ] Hallucination protection
- [ ] Conversation history

## Phase 5 — Production

- [ ] Authentication
- [ ] Authorization
- [ ] Logging
- [ ] Monitoring
- [ ] Error handling
- [ ] Caching
- [ ] Automated tests
- [ ] CI/CD
- [ ] Cloud deployment

---

# Git Commit Checkpoint

Before starting the LLM phase, commit the current stable implementation.

Check:

```powershell
git status
```

Add:

```powershell
git add .
```

Commit:

```powershell
git commit -m "Add graph context explorer and query support"
```

Push:

```powershell
git push origin main
```

For documentation changes:

```powershell
git add README.md .gitignore
git commit -m "Update project documentation and gitignore"
git push origin main
```

---

# Current Project Status

Current stable functionality:

```text
CognoDB
   |
   v
ASP.NET Core
   |
   v
Graph Retrieval
   |
   v
Intent Detection
   |
   v
Context Search
   |
   v
Angular
   |
   v
Cytoscape Graph
   |
   v
Interactive Context Explorer
```

## Next Step

The next planned development phase is:

**LLM + Graph RAG**

The current stable version should be committed to Git before starting that phase.
