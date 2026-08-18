using ContextGraph.Api.Models;
using Neo4j.Driver;

namespace ContextGraph.Api.Services;

public class ContextQueryService : IContextQueryService
{
    private readonly IDriver _driver;

    public ContextQueryService(IDriver driver)
    {
        _driver = driver;
    }


    public async Task<ContextQueryResponse> QueryContextAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(query));
        }


        // ---------------------------------------------
        // Detect query intent
        // ---------------------------------------------

        var intent = DetectIntent(query);

        var words = ExtractSearchWords(query);


        // ---------------------------------------------
        // Base response
        // ---------------------------------------------

        var response = new ContextQueryResponse
        {
            Query = query,

            Intent = intent.ToString()
        };


        // ---------------------------------------------
        // Execute query based on intent
        // ---------------------------------------------

        switch (intent)
        {
            case ContextQueryIntent.WorkingOn:

                return await FindWorkingOnAsync(
                    query,
                    words,
                    response,
                    cancellationToken);


            case ContextQueryIntent.Tasks:

                return await FindTasksAsync(
                    query,
                    words,
                    response,
                    cancellationToken);


            case ContextQueryIntent.Meeting:

                return await FindMeetingContextAsync(
                    query,
                    words,
                    response,
                    cancellationToken);


            case ContextQueryIntent.Decisions:

                return await FindDecisionsAsync(
                    query,
                    words,
                    response,
                    cancellationToken);


            case ContextQueryIntent.WhoWorking:

                return await FindWhoIsWorkingAsync(
                    query,
                    words,
                    response,
                    cancellationToken);


            default:

                return await FindGeneralContextAsync(
                    query,
                    words,
                    response,
                    cancellationToken);
        }
    }


    // ==================================================
    // INTENT DETECTION
    // ==================================================

    private ContextQueryIntent DetectIntent(string query)
    {
        var text = query.ToLowerInvariant();


        // Who is working on Phoenix?
        if (
            text.Contains("who") &&
            (
                text.Contains("working") ||
                text.Contains("work")
            )
        )
        {
            return ContextQueryIntent.WhoWorking;
        }


        // What is Satyam working on?
        if (
            text.Contains("working on") ||
            text.Contains("work on") ||
            text.Contains("doing")
        )
        {
            return ContextQueryIntent.WorkingOn;
        }


        // What tasks are assigned to Satyam?
        if (
            text.Contains("task") ||
            text.Contains("assigned")
        )
        {
            return ContextQueryIntent.Tasks;
        }


        // What happened in the meeting?
        if (
            text.Contains("meeting") ||
            text.Contains("sprint planning") ||
            text.Contains("happened")
        )
        {
            return ContextQueryIntent.Meeting;
        }


        // What decisions were made?
        if (
            text.Contains("decision") ||
            text.Contains("decisions") ||
            text.Contains("decided")
        )
        {
            return ContextQueryIntent.Decisions;
        }


        return ContextQueryIntent.General;
    }


    // ==================================================
    // SEARCH WORDS
    // ==================================================

    private List<string> ExtractSearchWords(
        string query)
    {
        var stopWords = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "what",
            "is",
            "are",
            "the",
            "a",
            "an",
            "who",
            "where",
            "when",
            "which",
            "working",
            "work",
            "on",
            "with",
            "for",
            "in",
            "of",
            "did",
            "does",
            "do",
            "has",
            "have",
            "about",
            "tell",
            "me",
            "show",
            "doing",
            "task",
            "tasks",
            "assigned",
            "to",
            "was",
            "were",
            "made",
            "happened"
        };


        return query
            .Split(
                new[]
                {
                    ' ',
                    '?',
                    '!',
                    '.',
                    ','
                },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x =>
                x.Length >= 2 &&
                !stopWords.Contains(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }


    // ==================================================
    // WORKING ON
    // ==================================================

    private async Task<ContextQueryResponse>
        FindWorkingOnAsync(
            string query,
            List<string> words,
            ContextQueryResponse response,
            CancellationToken cancellationToken)
    {
        await using var session =
            _driver.AsyncSession();


        var cursor = await session.RunAsync(
            """
            MATCH (p:Person)-[r:WORKS_ON]->(project:Project)
            WHERE ANY(word IN $words
                WHERE
                    toLower(p.name) CONTAINS toLower(word)
                    OR
                    toLower(p.id) CONTAINS toLower(word)
            )
            RETURN
                p.id AS personId,
                p.name AS personName,
                project.id AS nodeId,
                project.name AS nodeName
            """,
            new
            {
                words
            });


        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;


            response.Person = new ContextPerson
            {
                Id = record["personId"].As<string>(),

                Name = record["personName"].As<string>()
            };


            response.Connections.Add(
                new ContextConnection
                {
                    Relationship = "WORKS_ON",

                    NodeId =
                        record["nodeId"].As<string>(),

                    NodeName =
                        record["nodeName"].As<string>(),

                    NodeType = "Project"
                });
        }


        return response;
    }


    // ==================================================
    // TASKS
    // ==================================================

    private async Task<ContextQueryResponse>
        FindTasksAsync(
            string query,
            List<string> words,
            ContextQueryResponse response,
            CancellationToken cancellationToken)
    {
        await using var session =
            _driver.AsyncSession();


        var cursor = await session.RunAsync(
            """
            MATCH (p:Person)-[r:ASSIGNED_TO]->(task:Task)
            WHERE ANY(word IN $words
                WHERE
                    toLower(p.name) CONTAINS toLower(word)
                    OR
                    toLower(p.id) CONTAINS toLower(word)
            )
            RETURN
                p.id AS personId,
                p.name AS personName,
                task.id AS nodeId,
                task.title AS nodeName,
                task.status AS status
            """,
            new
            {
                words
            });


        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;


            if (response.Person == null)
            {
                response.Person =
                    new ContextPerson
                    {
                        Id =
                            record["personId"]
                                .As<string>(),

                        Name =
                            record["personName"]
                                .As<string>()
                    };
            }


            var taskName =
                record["nodeName"].As<string>();


            var status =
                record["status"].As<string>();


            response.Connections.Add(
                new ContextConnection
                {
                    Relationship =
                        $"ASSIGNED_TO ({status})",

                    NodeId =
                        record["nodeId"].As<string>(),

                    NodeName =
                        taskName,

                    NodeType = "Task"
                });
        }


        return response;
    }


    // ==================================================
    // MEETING
    // ==================================================

    private async Task<ContextQueryResponse>
        FindMeetingContextAsync(
            string query,
            List<string> words,
            ContextQueryResponse response,
            CancellationToken cancellationToken)
    {
        await using var session =
            _driver.AsyncSession();


        var cursor = await session.RunAsync(
            """
            MATCH (meeting:Meeting)
            WHERE ANY(word IN $words
                WHERE
                    toLower(meeting.title)
                    CONTAINS toLower(word)
                    OR
                    toLower(meeting.id)
                    CONTAINS toLower(word)
            )
            MATCH (meeting)-[r]-(node)
            RETURN
                meeting.id AS meetingId,
                meeting.title AS meetingTitle,
                type(r) AS relationship,
                node.id AS nodeId,
                COALESCE(
                    node.name,
                    node.title,
                    node.description,
                    node.id
                ) AS nodeName,
                labels(node)[0] AS nodeType
            """,
            new
            {
                words
            });


        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;


            response.Connections.Add(
                new ContextConnection
                {
                    Relationship =
                        record["relationship"]
                            .As<string>(),

                    NodeId =
                        record["nodeId"]
                            .As<string>(),

                    NodeName =
                        record["nodeName"]
                            .As<string>(),

                    NodeType =
                        record["nodeType"]
                            .As<string>()
                });
        }


        return response;
    }


    // ==================================================
    // DECISIONS
    // ==================================================

    private async Task<ContextQueryResponse>
        FindDecisionsAsync(
            string query,
            List<string> words,
            ContextQueryResponse response,
            CancellationToken cancellationToken)
    {
        await using var session =
            _driver.AsyncSession();


        var cursor = await session.RunAsync(
            """
            MATCH (decision:Decision)
            WHERE ANY(word IN $words
                WHERE
                    toLower(decision.description)
                    CONTAINS toLower(word)
                    OR
                    toLower(decision.id)
                    CONTAINS toLower(word)
            )
            RETURN
                decision.id AS nodeId,
                decision.description AS nodeName
            """,
            new
            {
                words
            });


        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;


            response.Connections.Add(
                new ContextConnection
                {
                    Relationship = "DECISION",

                    NodeId =
                        record["nodeId"]
                            .As<string>(),

                    NodeName =
                        record["nodeName"]
                            .As<string>(),

                    NodeType = "Decision"
                });
        }


        return response;
    }


    // ==================================================
    // WHO IS WORKING
    // ==================================================

    private async Task<ContextQueryResponse>
        FindWhoIsWorkingAsync(
            string query,
            List<string> words,
            ContextQueryResponse response,
            CancellationToken cancellationToken)
    {
        await using var session =
            _driver.AsyncSession();


        var cursor = await session.RunAsync(
            """
            MATCH (person:Person)-[r:WORKS_ON]->(project:Project)
            WHERE ANY(word IN $words
                WHERE
                    toLower(project.name)
                    CONTAINS toLower(word)
                    OR
                    toLower(project.id)
                    CONTAINS toLower(word)
            )
            RETURN
                person.id AS personId,
                person.name AS personName,
                project.id AS projectId,
                project.name AS projectName
            """,
            new
            {
                words
            });


        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;


            response.Connections.Add(
                new ContextConnection
                {
                    Relationship = "WORKS_ON",

                    NodeId =
                        record["personId"]
                            .As<string>(),

                    NodeName =
                        record["personName"]
                            .As<string>(),

                    NodeType = "Person"
                });
        }


        return response;
    }


    // ==================================================
    // GENERAL
    // ==================================================

    private async Task<ContextQueryResponse>
        FindGeneralContextAsync(
            string query,
            List<string> words,
            ContextQueryResponse response,
            CancellationToken cancellationToken)
    {
        await using var session =
            _driver.AsyncSession();


        var cursor = await session.RunAsync(
            """
            MATCH (p:Person)-[r]-(n)
            WHERE ANY(word IN $words
                WHERE
                    toLower(p.name) CONTAINS toLower(word)
                    OR
                    toLower(p.id) CONTAINS toLower(word)
            )
            RETURN
                p.id AS personId,
                p.name AS personName,
                type(r) AS relationship,
                n.id AS nodeId,
                COALESCE(
                    n.name,
                    n.title,
                    n.description,
                    n.id
                ) AS nodeName,
                labels(n)[0] AS nodeType
            """,
            new
            {
                words
            });


        while (await cursor.FetchAsync())
        {
            var record = cursor.Current;


            if (response.Person == null)
            {
                response.Person =
                    new ContextPerson
                    {
                        Id =
                            record["personId"]
                                .As<string>(),

                        Name =
                            record["personName"]
                                .As<string>()
                    };
            }


            response.Connections.Add(
                new ContextConnection
                {
                    Relationship =
                        record["relationship"]
                            .As<string>(),

                    NodeId =
                        record["nodeId"]
                            .As<string>(),

                    NodeName =
                        record["nodeName"]
                            .As<string>(),

                    NodeType =
                        record["nodeType"]
                            .As<string>()
                });
        }


        return response;
    }
}