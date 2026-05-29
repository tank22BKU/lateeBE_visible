using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.UpdateExpert;

public sealed class UpdateExpertCommand : IRequest<Expert?>
{
    public string ExpertId { get; set; } = string.Empty;
    public string? Ssn { get; set; }
    public string? BioQuote { get; set; }
    public string? EducationDetail { get; set; }
    public string? TitlePosition { get; set; }
    public string? ExpertiseSkill { get; set; }
    public string? SocialLink { get; set; }
}

public sealed class UpdateExpertHandler : IRequestHandler<UpdateExpertCommand, Expert?>
{
    private readonly IUserRepository _repository;

    public UpdateExpertHandler(IUserRepository repository) => _repository = repository;

    public Task<Expert?> Handle(UpdateExpertCommand request, CancellationToken cancellationToken)
    {
        var expert = new Expert
        {
            ExpertId = request.ExpertId,
            Ssn = request.Ssn ?? string.Empty,
            BioQuote = request.BioQuote,
            EducationDetail = request.EducationDetail,
            TitlePosition = request.TitlePosition,
            ExpertiseSkill = request.ExpertiseSkill,
            SocialLink = request.SocialLink,
        };

        return _repository.UpdateExpertAsync(expert);
    }
}
