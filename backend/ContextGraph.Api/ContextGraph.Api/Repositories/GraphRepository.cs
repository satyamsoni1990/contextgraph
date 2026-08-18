using ContextGraph.Api.DTOs.Context;
using ContextGraph.Api.Models;
using Neo4j.Driver;

namespace ContextGraph.Api.Repositories;

public class GraphRepository : IGraphRepository
{
    private readonly IDriver _driver;

    public GraphRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<(Person Person, Project Project)> CreatePersonProjectAsync(
        Person person,
        Project project)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
            CREATE (p:Person {
                id: $personId,
                name: $personName
            })
            CREATE (project:Project {
                id: $projectId,
                name: $projectName
            })
            CREATE (p)-[:WORKS_ON]->(project)
            RETURN p, project
            """;

        var result = await session.RunAsync(
            cypher,
            new
            {
                personId = person.Id,
                personName = person.Name,
                projectId = project.Id,
                projectName = project.Name
            });

        var record = await result.SingleAsync();

        var personNode = record["p"].As<INode>();
        var projectNode = record["project"].As<INode>();

        return (
            new Person
            {
                Id = personNode["id"].As<string>(),
                Name = personNode["name"].As<string>()
            },
            new Project
            {
                Id = projectNode["id"].As<string>(),
                Name = projectNode["name"].As<string>()
            }
        );
    }

    public async Task<(Person Person, Project Project)> GetPersonProjectAsync(
    string personId)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (p:Person)-[:WORKS_ON]->(project:Project)
        WHERE p.id = $personId
        RETURN p, project
        """;

        var result = await session.RunAsync(
            cypher,
            new
            {
                personId
            });

        var record = await result.SingleAsync();

        var personNode = record["p"].As<INode>();
        var projectNode = record["project"].As<INode>();

        return (
            new Person
            {
                Id = personNode["id"].As<string>(),
                Name = personNode["name"].As<string>()
            },
            new Project
            {
                Id = projectNode["id"].As<string>(),
                Name = projectNode["name"].As<string>()
            }
        );
    }

    public async Task CreateProjectContextAsync(
    string projectId,
    Meeting meeting,
    Decision decision,
    TaskItem task)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (project:Project)
        WHERE project.id = $projectId

        CREATE (meeting:Meeting {
            id: $meetingId,
            title: $meetingTitle,
            date: $meetingDate
        })

        CREATE (decision:Decision {
            id: $decisionId,
            description: $decisionDescription
        })

        CREATE (task:Task {
            id: $taskId,
            title: $taskTitle,
            status: $taskStatus
        })

        CREATE (meeting)-[:ABOUT]->(project)
        CREATE (meeting)-[:DISCUSSED]->(decision)
        CREATE (decision)-[:CREATED]->(task)

        RETURN meeting, decision, task
        """;

        await session.RunAsync(
            cypher,
            new
            {
                projectId,
                meetingId = meeting.Id,
                meetingTitle = meeting.Title,
                meetingDate = meeting.Date.ToString("yyyy-MM-dd"),
                decisionId = decision.Id,
                decisionDescription = decision.Description,
                taskId = task.Id,
                taskTitle = task.Title,
                taskStatus = task.Status
            });
    }

    public async Task<object> GetProjectContextAsync(string projectId)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (project:Project)<-[:ABOUT]-(meeting:Meeting)
              -[:DISCUSSED]->(decision:Decision)
              -[:CREATED]->(task:Task)
        WHERE project.id = $projectId
        RETURN project, meeting, decision, task
        """;

        var result = await session.RunAsync(
            cypher,
            new
            {
                projectId
            });

        var records = await result.ToListAsync();

        return records.Select(record => new
        {
            Project = new
            {
                Id = record["project"].As<INode>()["id"].As<string>(),
                Name = record["project"].As<INode>()["name"].As<string>()
            },
            Meeting = new
            {
                Id = record["meeting"].As<INode>()["id"].As<string>(),
                Title = record["meeting"].As<INode>()["title"].As<string>(),
                Date = record["meeting"].As<INode>()["date"].As<string>()
            },
            Decision = new
            {
                Id = record["decision"].As<INode>()["id"].As<string>(),
                Description = record["decision"].As<INode>()["description"].As<string>()
            },
            Task = new
            {
                Id = record["task"].As<INode>()["id"].As<string>(),
                Title = record["task"].As<INode>()["title"].As<string>(),
                Status = record["task"].As<INode>()["status"].As<string>()
            }
        }).ToList();
    }

    public async Task ConnectPersonToProjectContextAsync(
    string personId,
    string meetingId,
    string taskId)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (person:Person)
        WHERE person.id = $personId

        MATCH (meeting:Meeting)
        WHERE meeting.id = $meetingId

        MATCH (task:Task)
        WHERE task.id = $taskId

        CREATE (person)-[:ATTENDED]->(meeting)
        CREATE (person)-[:ASSIGNED_TO]->(task)
        """;

        await session.RunAsync(
            cypher,
            new
            {
                personId,
                meetingId,
                taskId
            });
    }

    public async Task<object> GetPersonContextAsync(string personId)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (person:Person)-[:ATTENDED]->(meeting:Meeting)
              -[:DISCUSSED]->(decision:Decision)
              -[:CREATED]->(task:Task)
        WHERE person.id = $personId
        RETURN person, meeting, decision, task
        """;

        var result = await session.RunAsync(
            cypher,
            new
            {
                personId
            });

        var records = await result.ToListAsync();

        return records.Select(record =>
        {
            var personNode = record["person"].As<INode>();
            var meetingNode = record["meeting"].As<INode>();
            var decisionNode = record["decision"].As<INode>();
            var taskNode = record["task"].As<INode>();

            return new
            {
                Person = new
                {
                    Id = personNode["id"].As<string>(),
                    Name = personNode["name"].As<string>()
                },

                Meeting = new
                {
                    Id = meetingNode["id"].As<string>(),
                    Title = meetingNode["title"].As<string>(),
                    Date = meetingNode["date"].As<string>()
                },

                Decision = new
                {
                    Id = decisionNode["id"].As<string>(),
                    Description = decisionNode["description"].As<string>()
                },

                Task = new
                {
                    Id = taskNode["id"].As<string>(),
                    Title = taskNode["title"].As<string>(),
                    Status = taskNode["status"].As<string>()
                }
            };
        }).ToList();
    }

    public async Task CreateProjectArtifactsAsync(
    string projectId,
    string personId,
    Document document,
    Email email)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (project:Project)
        WHERE project.id = $projectId

        MATCH (person:Person)
        WHERE person.id = $personId

        CREATE (document:Document {
            id: $documentId,
            title: $documentTitle,
            type: $documentType
        })

        CREATE (email:Email {
            id: $emailId,
            subject: $emailSubject,
            summary: $emailSummary
        })

        CREATE (person)-[:AUTHORED]->(document)
        CREATE (document)-[:BELONGS_TO]->(project)

        CREATE (person)-[:SENT]->(email)
        CREATE (email)-[:ABOUT]->(project)

        RETURN document, email
        """;

        await session.RunAsync(
            cypher,
            new
            {
                projectId,
                personId,

                documentId = document.Id,
                documentTitle = document.Title,
                documentType = document.Type,

                emailId = email.Id,
                emailSubject = email.Subject,
                emailSummary = email.Summary
            });
    }

    public async Task<ProjectContextDto?> GetFullProjectContextAsync(
      string projectId)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (project:Project)
        WHERE project.id = $projectId

        OPTIONAL MATCH (person:Person)-[:WORKS_ON]->(project)
        WITH project,
             collect(DISTINCT person) AS people

        OPTIONAL MATCH (meeting:Meeting)-[:ABOUT]->(project)
        WITH project,
             people,
             collect(DISTINCT meeting) AS meetings

        OPTIONAL MATCH (meeting2:Meeting)-[:ABOUT]->(project)
        OPTIONAL MATCH (meeting2)-[:DISCUSSED]->(decision:Decision)
        OPTIONAL MATCH (decision)-[:CREATED]->(task:Task)
        WITH project,
             people,
             meetings,
             collect(DISTINCT decision) AS decisions,
             collect(DISTINCT task) AS tasks

        OPTIONAL MATCH (document:Document)-[:BELONGS_TO]->(project)
        WITH project,
             people,
             meetings,
             decisions,
             tasks,
             collect(DISTINCT document) AS documents

        OPTIONAL MATCH (email:Email)-[:ABOUT]->(project)
        WITH project,
             people,
             meetings,
             decisions,
             tasks,
             documents,
             collect(DISTINCT email) AS emails

        RETURN project,
               people,
               meetings,
               decisions,
               tasks,
               documents,
               emails
        """;

        var result = await session.RunAsync(
            cypher,
            new
            {
                projectId
            });

        // Neo4j.Driver version compatibility:
        // IResultCursor supports ToListAsync(), so use it
        // instead of SingleOrDefaultAsync().
        var records = await result.ToListAsync();

        var record = records.FirstOrDefault();

        if (record == null)
        {
            return null;
        }

        var projectNode = record["project"].As<INode>();

        var response = new ProjectContextDto
        {
            Project = new ProjectContextProjectDto
            {
                Id = projectNode["id"].As<string>(),
                Name = projectNode["name"].As<string>()
            }
        };

        // People
        foreach (var node in record["people"].As<List<INode>>())
        {
            response.People.Add(new ProjectContextPersonDto
            {
                Id = node["id"].As<string>(),
                Name = node["name"].As<string>()
            });
        }

        // Meetings
        foreach (var node in record["meetings"].As<List<INode>>())
        {
            response.Meetings.Add(new ProjectContextMeetingDto
            {
                Id = node["id"].As<string>(),
                Title = node["title"].As<string>(),
                Date = node["date"].As<string>()
            });
        }

        // Decisions
        foreach (var node in record["decisions"].As<List<INode>>())
        {
            response.Decisions.Add(new ProjectContextDecisionDto
            {
                Id = node["id"].As<string>(),
                Description = node["description"].As<string>()
            });
        }

        // Tasks
        foreach (var node in record["tasks"].As<List<INode>>())
        {
            response.Tasks.Add(new ProjectContextTaskDto
            {
                Id = node["id"].As<string>(),
                Title = node["title"].As<string>(),
                Status = node["status"].As<string>()
            });
        }

        // Documents
        foreach (var node in record["documents"].As<List<INode>>())
        {
            response.Documents.Add(new ProjectContextDocumentDto
            {
                Id = node["id"].As<string>(),
                Title = node["title"].As<string>(),
                Type = node["type"].As<string>()
            });
        }

        // Emails
        foreach (var node in record["emails"].As<List<INode>>())
        {
            response.Emails.Add(new ProjectContextEmailDto
            {
                Id = node["id"].As<string>(),
                Subject = node["subject"].As<string>(),
                Summary = node["summary"].As<string>()
            });
        }

        return response;
    }

    public async Task<GraphExplorerDto> GetGraphExplorerAsync(
     string projectId)
    {
        await using var session = _driver.AsyncSession();

        const string cypher = """
        MATCH (project:Project)
        WHERE project.id = $projectId

        OPTIONAL MATCH path = (project)-[*1..4]-(connected)

        UNWIND relationships(path) AS rel

        WITH project,
             collect(DISTINCT connected) AS connectedNodes,
             collect(DISTINCT {
                 source: startNode(rel).id,
                 target: endNode(rel).id,
                 type: type(rel)
             }) AS relationships

        RETURN project,
               connectedNodes,
               relationships
        """;

        var result = await session.RunAsync(
            cypher,
            new
            {
                projectId
            });

        var records = await result.ToListAsync();
        var record = records.FirstOrDefault();

        var response = new GraphExplorerDto();

        if (record == null)
        {
            return response;
        }

        AddNode(response, record["project"].As<INode>());

        foreach (var node in record["connectedNodes"].As<List<INode>>())
        {
            AddNode(response, node);
        }

        var relationships =
            record["relationships"]
                .As<List<Dictionary<string, object>>>();

        foreach (var relationship in relationships)
        {
            if (relationship["source"] == null ||
                relationship["target"] == null)
            {
                continue;
            }

            var source = relationship["source"].As<string>();
            var target = relationship["target"].As<string>();
            var type = relationship["type"].As<string>();

            if (!response.Relationships.Any(x =>
                x.Source == source &&
                x.Target == target &&
                x.Type == type))
            {
                response.Relationships.Add(
                    new GraphRelationshipDto
                    {
                        Source = source,
                        Target = target,
                        Type = type
                    });
            }
        }

        return response;
    }

    private static void AddNode(
    GraphExplorerDto response,
    INode node)
    {
        var businessId = node.Properties
            .TryGetValue("id", out var idValue)
                ? idValue.As<string>()
                : node.ElementId;

        if (response.Nodes.Any(x => x.Id == businessId))
        {
            return;
        }

        var label = node.Labels.FirstOrDefault()
                    ?? "Unknown";

        string displayName;

        if (node.Properties.TryGetValue("name", out var name))
        {
            displayName = name.As<string>();
        }
        else if (node.Properties.TryGetValue("title", out var title))
        {
            displayName = title.As<string>();
        }
        else if (node.Properties.TryGetValue(
                     "description", out var description))
        {
            displayName = description.As<string>();
        }
        else if (node.Properties.TryGetValue(
                     "subject", out var subject))
        {
            displayName = subject.As<string>();
        }
        else
        {
            displayName = businessId;
        }

        response.Nodes.Add(new GraphNodeDto
        {
            Id = businessId,
            Label = label,
            DisplayName = displayName
        });
    }
}