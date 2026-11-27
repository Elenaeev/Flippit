using Flippit.Api.BL.Facades;
using Flippit.Common.Models.CompletedLesson;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Flippit.Api.App.Endpoints;

public static class CompletedLessonEndpoints
{
    public static RouteGroupBuilder MapCompletedLessonEndpoints(this RouteGroupBuilder routeGroupBuilder)
    {
        var completedLessonsEndPoints = routeGroupBuilder.MapGroup("completedLessons")
            .WithTags("completedLessons");

        completedLessonsEndPoints.MapGet("", (ICompletedLessonFacade facade, [FromQuery] int pageSize = 10, [FromQuery] int page = 1, [FromQuery] string? filter = null, [FromQuery] string? sortBy = null) =>
        {
            var lessons = facade.GetAll(filter, sortBy, page, pageSize);
            return TypedResults.Ok(lessons);
        });

        completedLessonsEndPoints.MapGet("/{id:guid}", (ICompletedLessonFacade facade, Guid id) =>
        {
            var lesson = facade.GetById(id);
            return TypedResults.Ok(lesson);
        });

        completedLessonsEndPoints.MapPost("", [Authorize] async Task<Results<Ok<Guid>, IResult>> (ICompletedLessonFacade facade, CompletedLessonDetailModel model, HttpContext httpContext) =>
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            if (model.UserId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }

            var id = facade.Create(model);
            return TypedResults.Ok(id);
        });

        completedLessonsEndPoints.MapPut("", [Authorize(Roles = "Admin")] (ICompletedLessonFacade facade, CompletedLessonDetailModel model) =>
        {
            var id = facade.Update(model);
            return TypedResults.Ok(id);
        });

        completedLessonsEndPoints.MapPost("upsert", [Authorize] async Task<Results<Ok<Guid>, IResult>> (ICompletedLessonFacade facade, CompletedLessonDetailModel model, HttpContext httpContext) =>
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            if(model.UserId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }

            var id = facade.CreateOrUpdate(model);
            return TypedResults.Ok(id);
        });

        completedLessonsEndPoints.MapDelete("/{id:guid}", [Authorize(Roles = "Admin")] (ICompletedLessonFacade facade, Guid id) =>
        {
            facade.Delete(id);
        });

        return routeGroupBuilder;
    }
}
