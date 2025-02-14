using Carter;
using FluentValidation;
using h.Contracts.Games;
using h.Primitives.Games;
using h.Server.Entities.Games;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games.TdaApiSpecNecessary;

public static class CreateNewGame
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/games", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromBody] CreateNewGameRequest request,
        [FromServices] AppDbContext db,
        [FromServices] IValidator<CreateNewGameRequest> validator)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationError(validationResult);

        // Map to entity
        var boardResult = GameBoard.Parse(request.Board);
        if (boardResult.IsError)
        {
            // -> Error while parsing
            return ErrorResults.ValidationError(boardResult.Errors);
        }

        var board = boardResult.Value;
        var gameResult = Game.CreateNewGame(request.Name, request.Difficulty, board);

        if (gameResult.IsError)
            return ErrorResults.ValidationError(gameResult.Errors);

        var game = gameResult.Value;

        // Persist
        await db.GamesDbSet.AddAsync(game);
        await db.SaveChangesAsync();

        // Map to response
        var response = new GameResponse(
            game.Id,
            game.CreatedAt,
            game.UpdatedAt,
            game.Name,
            game.Difficulty,
            game.GameState,
            game.Board.BoardMatrixToString()
        );

        return Results.Created($"/api/games/{game.Id}", response);
    }

    public class Validator : AbstractValidator<CreateNewGameRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Difficulty).IsInSmartEnum(GameDifficulty.List);
            RuleFor(x => x.Board).NotEmpty();
        }
    }
}