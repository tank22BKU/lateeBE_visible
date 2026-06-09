using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQuery : IRequest<UserDto?>
{
    public string UserId { get; set; } = string.Empty;
}

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return null;
        }

        var result = new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Birthday = user.Birthday,
            Gender = user.Gender,
            Address = user.Address,
            Status = user.Status,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Profile = null
        };

        if (string.Equals(user.Role, "expert", StringComparison.OrdinalIgnoreCase))
        {
            var expert = await _repository.GetExpertByIdAsync(user.UserId);
            if (expert is not null)
            {
                result.Profile = new Profile
                {
                    Id = expert.ExpertId,
                    Ssn = expert.Ssn,
                    BioQoute = expert.BioQuote,
                    TitlePosition = expert.TitlePosition,
                    EducationDetail = expert.EducationDetail,
                    ExpertiseSkill = expert.ExpertiseSkill,
                    SocialLink = expert.SocialLink
                };
            }
        }

        return result;
    }
}