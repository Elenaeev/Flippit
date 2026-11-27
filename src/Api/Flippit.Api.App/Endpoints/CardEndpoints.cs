using Flippit.Api.App.Helpers;
using Flippit.Api.BL.Facades;
using Flippit.Common.Models.Card;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Flippit.Api.App.Endpoints;

public static class CardEndpoints
{
    public static RouteGroupBuilder MapCardEndpoints(this RouteGroupBuilder routeGroupBuilder)
    {
        var cardEndPoints = routeGroupBuilder.MapGroup("cards")
            .WithTags("cards");

        cardEndPoints.MapGet("", (ICardFacade cardFacade, [FromQuery] string? filter = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null) =>
        {
            var cards = cardFacade.GetAll(filter, sortBy, page, pageSize);
            return TypedResults.Ok(cards);
        });

        cardEndPoints.MapGet("/{id:guid}",
           Results<Ok<CardDetailModel>, NotFound<string>> (
               Guid id,
               [FromServices] ICardFacade cardFacade
           )
           =>
           cardFacade.GetById(id) is { } card
               ? TypedResults.Ok(card)
               : TypedResults.NotFound($"Card with ID {id} was not found.")
        );

        cardEndPoints.MapPost("", [Authorize] async Task<Results<Ok<Guid>, ValidationProblem, IResult>> (ICardFacade CardFacade, CardDetailModel model, IValidator<CardDetailModel> validator, HttpContext httpContext) =>
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

            if (string.IsNullOrEmpty(userId))
            {
                return TypedResults.Unauthorized();
            }

            if (model.CreatorId != Guid.Parse(userId))
            {
                return TypedResults.Unauthorized();
            }

            var id = CardFacade.Create(model);
            return TypedResults.Ok(id);
        });

        cardEndPoints.MapPut("", [Authorize] async Task<Results<Ok<Guid?>, ValidationProblem, IResult>> (ICardFacade CardFacade, CardDetailModel model, IValidator<CardDetailModel> validator, HttpContext httpContext) =>
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

            var id = CardFacade.Update(model);
            return TypedResults.Ok(id);
        });

        cardEndPoints.MapPost("upsert", [Authorize] async Task<Results<Ok<Guid>, ValidationProblem, IResult>> (ICardFacade CardFacade, CardDetailModel model, IValidator<CardDetailModel> validator, HttpContext httpContext) =>
        {
            var validationErrors = await ValidationHelper.ValidateModelAsync(model, validator);
            if (validationErrors != null)
            {
                return TypedResults.ValidationProblem(validationErrors);
            }
            //var id = CardFacade.CreateOrUpdate(model);
            //return TypedResults.Ok(id);

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

            if (model.CreatorId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }

            var id = CardFacade.CreateOrUpdate(model);
            return TypedResults.Ok(id);
        });

        cardEndPoints.MapDelete("/{id:guid}", [Authorize] async Task<Results<Ok<Guid>, IResult>> (ICardFacade cardFacade, Guid id, HttpContext httpContext) =>
        {

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length);
            }

            var userRole = httpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = httpContext.User.FindFirst("sub")?.Value
                  ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var card = cardFacade.GetById(id);
            if (card == null)
            {
                return TypedResults.NotFound($"Card with ID {id} was not found.");
            }

            if (card.CreatorId != Guid.Parse(userId) && userRole != "Admin")
            {
                return TypedResults.Unauthorized();
            }

            cardFacade.Delete(id);
            return TypedResults.Ok();
        });

        return routeGroupBuilder;
    }
}
