
# ContextGraph — Project Checkpoint

## 1. Project Overview

ContextGraph is an AI-powered Work Context Explorer built around:

- ASP.NET Core / C#
- Angular / TypeScript
- CognoDB graph database
- Cypher
- Cytoscape.js
- Neo4j-compatible .NET driver

The goal is to connect people, projects, meetings, decisions, tasks, documents and emails into a connected work-context graph.

---

# 2. Current Architecture

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

# 3. Graph Model

Current entities:

```text
Person
Project
Meeting
Decision
Task
Document
Email
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

Current graph demonstrated:

- 7 nodes
- 10 relationships

---

# 4. Backend

## Project

```text
backend/
└── ContextGraph.Api/
```

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

# 5. CognoDB

The application connects to CognoDB through the Neo4j-compatible .NET driver.

Cypher is used to query graph relationships.

Example graph concept:

```cypher
MATCH (p:Person)-[r:WORKS_ON]->(project:Project)
RETURN p, r, project
```

---

# 6. Working Graph API

The API can retrieve project context.

Example:

```http
GET /api/Graph/explorer/PR001
```

The API returns graph nodes and relationships.

---

# 7. Context Query API

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

Current backend flow:

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

---

# 8. Query Intent Detection

Current rule-based intents:

```text
General
WorkingOn
Tasks
Meeting
Decisions
WhoWorking
```

Examples:

| Question                                  | Intent     |
| ----------------------------------------- | ---------- |
| What is Satyam working on?                | WorkingOn  |
| What tasks are assigned to Satyam?        | Tasks      |
| What happened in Phoenix Sprint Planning? | Meeting    |
| What decisions were made?                 | Decisions  |
| Who is working on Phoenix?                | WhoWorking |

This is intentionally rule-based at the current stage.

---

# 9. Important Driver Fix

The installed driver version does not support:

```csharp
SingleOrDefaultAsync()
```

on `IResultCursor`.

The working approach is:

```csharp
var hasRecord = await cursor.FetchAsync();

if (hasRecord)
{
    var record = cursor.Current;
}
```

For multiple records:

```csharp
while (await cursor.FetchAsync())
{
    var record = cursor.Current;

    // process record
}
```

This should be kept in mind when adding new CognoDB queries.

---

# 10. Angular Frontend

Frontend project:

```text
frontend/
└── contextgraph-ui/
```

Important area:

```text
src/app/
├── core/
├── models/
└── context-explorer/
```

The Context Explorer uses Cytoscape.js to display the graph.

---

# 11. Cytoscape Graph

The graph supports:

- Zoom
- Pan
- Node movement
- Relationship labels
- Node selection
- Node highlighting
- Node details

Node colors/types include:

```text
Project
Person
Meeting
Decision
Task
Document
Email
```

Clicking a node displays:

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

# 12. Angular Context Search

The UI contains:

```text
Ask about your work context

[ What is Satyam working on? ] [ Ask Context ]
```

Angular calls:

```text
POST /api/Graph/query-context
```

The response displays:

- Query
- Intent
- Person
- Connected context
- Relationship
- Node type
- Node ID

---

# 13. FormsModule

Because Context Explorer is a standalone Angular component, FormsModule is imported directly in the component.

```typescript
import { FormsModule } from '@angular/forms';
```

Component:

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

# 14. Angular Context Models

Current model:

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

# 15. Current Successful Test

The following question works:

```text
What is Satyam working on?
```

The backend returns context including:

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
E001
```

The Angular UI successfully displays this context.

---

# 16. Current Stable Architecture

```text
User
 |
 v
Angular Context Explorer
 |
 | HTTP POST
 v
ASP.NET Core
 |
 v
ContextQueryService
 |
 v
Intent Detection
 |
 v
Cypher
 |
 v
CognoDB
 |
 v
Structured Context
 |
 v
Angular UI
```

This is the current stable checkpoint.

---

# 17. Git / .gitignore

Create `.gitignore` at the repository root.

Recommended contents:

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

Do not commit:

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

# 18. Git Commit Checkpoint

Before commit:

```powershell
git status
```

Add files:

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

A documentation-only follow-up commit can be:

```powershell
git add README.md .gitignore
git commit -m "Update project documentation and gitignore"
git push origin main
```

---

# 19. Business Use Case

ContextGraph represents a work-context intelligence system.

Example question:

```text
What is Satyam working on?
```

The graph can connect:

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

This provides connected context instead of isolated records.

Potential business questions:

- What is a person currently working on?
- What tasks are assigned to a person?
- Who is working on a project?
- What happened in a meeting?
- What decisions were made?
- Which documents are related to a project?
- Which emails are related to a work item?
- What context should a user know before taking an action?

---

# 20. Next Phase — AI / Graph RAG

The LLM integration was intentionally postponed.

Planned architecture:

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
                 Verified Context
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

Important principle:

The LLM should not invent business information.

The graph should provide the relevant business context first, and the LLM should reason over that retrieved context.

---

# 21. Future Roadmap

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

# 22. Current Checkpoint

STOP HERE BEFORE STARTING LLM WORK.

Stable functionality currently includes:

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
```

The next development step is:

```text
Step 19 — LLM / Graph RAG
```

Before starting Step 19, commit the current stable code to Git.
