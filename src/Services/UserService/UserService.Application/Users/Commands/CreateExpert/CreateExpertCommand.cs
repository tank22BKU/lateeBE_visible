using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.CreateExpert;

public sealed class CreateExpertCommand : IRequest<Expert?>
{
    public string ExpertId { get; set; } = string.Empty; 
    public string Ssn { get; set; } = string.Empty;
    public string? BioQuote { get; set; }
    public string? EducationDetail { get; set; }
    public string? TitlePosition { get; set; }
    public string? ExpertiseSkill { get; set; }
    public string? SocialLink { get; set; }
}

public sealed class CreateExpertHandler : IRequestHandler<CreateExpertCommand, Expert?>
{
    private readonly IUserRepository _repository;

    public CreateExpertHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<Expert?> Handle(CreateExpertCommand request, CancellationToken cancellationToken)
    {
        var expert = new Expert
        {
            ExpertId = request.ExpertId,
            Ssn = request.Ssn,
            BioQuote = request.BioQuote,
            EducationDetail = request.EducationDetail,
            TitlePosition = request.TitlePosition,
            ExpertiseSkill = request.ExpertiseSkill,
            SocialLink = request.SocialLink,
        };

        return _repository.CreateExpertAsync(expert);
    }
}
