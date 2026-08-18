using ContextGraph.Api.DTOs.Context;
using ContextGraph.Api.Models;

namespace ContextGraph.Api.Repositories;

public interface IGraphRepository
{
    Task<(Person Person, Project Project)> CreatePersonProjectAsync(
        Person person,
        Project project);

    Task<(Person Person, Project Project)> GetPersonProjectAsync(
        string personId);

    Task CreateProjectContextAsync(
        string projectId,
        Meeting meeting,
        Decision decision,
        TaskItem task);

    Task<object> GetProjectContextAsync(string projectId);

    Task ConnectPersonToProjectContextAsync(
    string personId,
    string meetingId,
    string taskId);

    Task<object> GetPersonContextAsync(string personId);

    Task CreateProjectArtifactsAsync(
    string projectId,
    string personId,
    Document document,
    Email email);

    Task<ProjectContextDto?> GetFullProjectContextAsync(
    string projectId);
    Task<GraphExplorerDto> GetGraphExplorerAsync(
    string projectId);
}