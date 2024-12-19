using Carter;
using FluentValidation;
using h.Contracts.Games;
using h.Primitives.Games;
using h.Server.Entities.Games;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games;

public static class UpdateGame
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/v1/games/{id}", Handle);
        }
    }

    public static async Task<IResult> Handle(
                [FromRoute] Guid id,
                [FromBody] UpdateGameRequest request,
                [FromServices] AppDbContext db,
                [FromServices] IValidator<UpdateGameRequest> validator)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationError(validationResult);

        var game = await db.GamesDbSet.FindAsync(id);

        if (game is null)
            return ErrorResults.NotFound();

        var newBoardResult = GameBoard.Parse(request.Board);
        if (newBoardResult.IsError)
            return ErrorResults.ValidationError(newBoardResult.Errors);

        // Update properties
        game.Update(
            request.Name,
            request.Difficulty,
            newBoardResult.Value
        );

        // Persist
        db.GamesDbSet.Update(game);
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

        return Results.Ok(response);
    }

    public class Validator : AbstractValidator<UpdateGameRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Difficulty).IsInSmartEnum(GameDifficulty.List);
            RuleFor(x => x.Board).NotEmpty();
        }
    }
}
