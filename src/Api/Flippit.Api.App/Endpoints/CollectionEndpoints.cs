using Flippit.Api.App.Helpers;
using Flippit.Api.BL.Facades;
using Flippit.Common.Models.Collection;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Flippit.Api.App.Endpoints;

public static class CollectionEndpoints
{
    public static RouteGroupBuilder MapCollectionEndpoints(this RouteGroupBuilder routeGroupBuilder)
    {
        var collectionsEndPoints = routeGroupBuilder.MapGroup("collections")
            .WithTags("collections");

        collectionsEndPoints.MapGet("", (ICollectionFacade collectionFacade, [FromQuery] string? filter = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null) =>
        {
            var collections = collectionFacade.GetAll(filter, sortBy, page, pageSize);
            return TypedResults.Ok(collections);
        });

        collectionsEndPoints.MapGet("/{id:guid}",
           Results<Ok<CollectionDetailModel>, NotFound<string>> (
               Guid id,
               [FromServices] ICollectionFacade collectionFacade
           )
           =>
           collectionFacade.GetById(id) is { } card
               ? TypedResults.Ok(card)
               : TypedResults.NotFound($"Collection with ID {id} was not found.")
        );

        collectionsEndPoints.MapGet("/{id:guid}/cards", (ICardFacade collectionFacade, Guid id, [FromQuery] string? filter = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null) =>
        {
            var cards = collectionFacade.SearchByCollectionId(id, filter, sortBy, page, pageSize);
            return TypedResults.Ok(cards);
        });

        collectionsEndPoints.MapGet("/{id:guid}/CompletedLessons", (ICompletedLessonFacade facade, Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null) =>
        {
            var AllCompletedLessons = facade.SearchByCollectionId(id, sortBy, page, pageSize);
            return TypedResults.Ok(AllCompletedLessons);
        });

        collectionsEndPoints.MapPost("", [Authorize] async Task<Results<Ok<Guid>, ValidationProblem, IResult>> (ICollectionFacade collectionFacade, CollectionDetailModel model, IValidator<CollectionDetailModel> validator, HttpContext httpContext) =>
        {
            var validationErrors = await ValidationHelper.ValidateModelAsync(model, validator);
            if (validationErrors != null)
            {
                return TypedResults.ValidationProblem(validationErrors);
            }

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if(string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            if (model.CreatorId != Guid.Parse(userId))
            {
                return TypedResults.Unauthorized();
            }

            var id = collectionFacade.Create(model);
            return TypedResults.Ok(id);
        });

        collectionsEndPoints.MapPut("", [Authorize] async Task<Results<Ok<Guid?>, ValidationProblem, IResult>> (ICollectionFacade collectionFacade, CollectionDetailModel model, IValidator<CollectionDetailModel> validator, HttpContext httpContext) =>
        {
            var validationErrors = await ValidationHelper.ValidateModelAsync(model, validator);
            if (validationErrors != null)
            {
                return TypedResults.ValidationProblem(validationErrors);
            }

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (model.CreatorId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }

            var id = collectionFacade.Update(model);
            return TypedResults.Ok(id);
        });

        collectionsEndPoints.MapPost("upsert", [Authorize] async Task<Results<Ok<Guid>, ValidationProblem, IResult>> (ICollectionFacade collectionFacade, CollectionDetailModel model, IValidator<CollectionDetailModel> validator, HttpContext httpContext) =>
        {
            var validationErrors = await ValidationHelper.ValidateModelAsync(model, validator);
            if (validationErrors != null)
            {
                return TypedResults.ValidationProblem(validationErrors);
            }

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if(string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            if(model.CreatorId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }

            var id = collectionFacade.CreateOrUpdate(model);
            return TypedResults.Ok(id);

        });

        collectionsEndPoints.MapDelete("/{id:guid}", [Authorize] async Task<Results<Ok<Guid>, IResult>> (ICollectionFacade collectionFacade, Guid id, HttpContext httpContext) =>
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var collection = collectionFacade.GetById(id);

            if(collection == null)
            {
                return TypedResults.NotFound($"Collection with ID {id} was not found.");
            }

            if (collection.CreatorId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }
            collectionFacade.Delete(id);
            return TypedResults.Ok();
        });

        return routeGroupBuilder;
    }
}
