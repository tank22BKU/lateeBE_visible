using MediatR;
using UserService.Application.Common.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.UpdateAvatarImage;

public sealed class UpdateAvatarImageCommand : IRequest<User?>
{
    public string UserId { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}

public sealed class UpdateAvatarImageHandler : IRequestHandler<UpdateAvatarImageCommand, User?>
{
    private readonly IUserRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public UpdateAvatarImageHandler(IUserRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<User?> Handle(UpdateAvatarImageCommand request, CancellationToken cancellationToken)
    {
        var avatarUrl = await _fileStorageService.UploadFileAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);

        var entity = new User
        {
            UserId = request.UserId,
            AvatarUrl = avatarUrl,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repository.UpdateUserAsync(entity);
    }
}