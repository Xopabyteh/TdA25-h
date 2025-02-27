﻿using h.Contracts;
using h.Contracts.AuditLog;
using h.Contracts.Auth;
using h.Contracts.Components.Services;
using h.Contracts.Leaderboard;
using h.Contracts.Users;
using Refit;

namespace h.Client.Services;

public interface IHApiClient : IWasmOnly
{
    // Auth
    [Post("/api/v1/users/login")]
    Task<ApiResponse<AuthenticationResponse>> LoginUser([Body] LoginUserRequest request);
    [Post("/api/v1/users/guest-login")]
    Task<ApiResponse<GuestLoginResponse>> GuestLogin();

    [Post("/api/v1/users/register")]
    Task<ApiResponse<AuthenticationResponse>> RegisterUser([Body] RegisterUserRequest request);
    [Post("/api/v1/users/logout")]
    Task LogoutUser();

    [Get("/api/v1/users/current")]
    Task<ApiResponse<UserResponse>> GetCurrentUser();
    [Get("/api/v1/users/current/claims")]
    Task<ClaimResponse[]> GetCurrentUserClaims();

    // User
    [Put("/api/v1/users/self")]
    Task<IApiResponse> UpdateUserSelf([Body] UpdateUserSelfRequest request);
    [Put("/api/v1/users/admin-update/{id}")]
    Task<IApiResponse<UserResponse>> AdminUpdateUser(Guid id, [Body] AdminUpdateUserRequest request);
    [Put("/api/v1/users/{uuid}/ban")]
    Task<IApiResponse> BanUserFromRankedMatchmaking(Guid uuid);
    [Put("/api/v1/users/{uuid}/unban")]
    Task<IApiResponse> UnbanUserFromRankedMatchmaking(Guid uuid);

    [Get("/api/v1/users/find")]
    Task<ApiResponse<UserResponse>> FindUser([Query] string query);
    [Delete("/api/v1/users/{id}")]
    Task<IApiResponse> DeleteUser(Guid id);

    // Invitation
    [Post("/api/v1/invitation/create")]
    Task<int> CreateInviteCode();

    [Post("/api/v1/invitation/join/{roomCode}")]
    Task<IApiResponse> JoinInviteRoom(int roomCode);

    // Matchmaking
    [Get("/api/v1/matchmaking/queue-size")]
    Task<int> GetQueueSize();

    [Post("/api/v1/matchmaking/join")]
    Task<int> JoinMatchmaking();

    [Post("/api/v1/matchmaking/leave")]
    Task<IApiResponse> LeaveMatchmaking();
    [Post("/api/v1/matchmaking/accept/{matchId}")]
    Task<IApiResponse> AcceptMatch(Guid matchId);
    [Post("/api/v1/matchmaking/decline/{matchId}")]
    Task<IApiResponse> DeclineMatch(Guid matchId);

    // Leaderboard
    [Get("/api/v1/leaderboard")]
    Task<LeaderBoardResponse> GetLeaderboard(int skip, int count);

    // Admin
    [Get("/api/v1/auditlog")]
    Task<AuditLogResponse> GetAuditLog(int skip, int count);
}
