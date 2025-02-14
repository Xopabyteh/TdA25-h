﻿using Carter;
using h.Contracts.Games;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Games.TdaApiSpecNecessary;

public static class GetAllGames
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/games", Handle);
        }
    }

    public static async Task<IResult> Handle([FromServices] AppDbContext db)
    {
        // Get from db
        var games = await db.GamesDbSet.Select(g => new
        {
            g.Id,
            g.CreatedAt,
            g.UpdatedAt,
            g.Name,
            g.Difficulty,
            g.GameState,
            g.Board
        }).ToListAsync();

        // Map to response
        var responses = games.Select(g => new GameResponse(
            g.Id,
            g.CreatedAt,
            g.UpdatedAt,
            g.Name,
            g.Difficulty,
            g.GameState,
            g.Board.BoardMatrixToString()
        ));

        return Results.Ok(responses);
    }
}
