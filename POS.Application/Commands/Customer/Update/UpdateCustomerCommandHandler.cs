using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Customer.Update;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEncryptionService _encryptionService;

    public UpdateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork uow, IMapper mapper, IEncryptionService encryptionService)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _encryptionService = encryptionService;
    }

    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Customer {request.Id} not found.");

        _mapper.Map(request.Dto, entity);

        // Normalize email and phone so empty strings are stored as NULL
        entity.Email = string.IsNullOrWhiteSpace(request.Dto.Email) ? null : request.Dto.Email.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(request.Dto.Phone) ? null : request.Dto.Phone.Trim();

        if (!string.IsNullOrWhiteSpace(entity.Email))
        {
            var exists = await _repository.GetQueryable()
                .AnyAsync(c => c.TenantId == entity.TenantId && c.Id != entity.Id && c.Email != null && c.Email.ToLower() == entity.Email.ToLower(), cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("A customer with this email address is already registered.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.IdentityNumber))
        {
            entity.EncryptedIdentityNumber = _encryptionService.Encrypt(request.Dto.IdentityNumber.Trim());
            entity.IdentityType = request.Dto.IdentityType;
        }

        if (request.Dto.LivenessVerified || !string.IsNullOrWhiteSpace(request.Dto.PhotoUrl))
        {
            entity.IsIdentityVerified = true;
            entity.LivenessVerifiedAt = DateTimeOffset.UtcNow;
            entity.LivenessAuditLog = request.Dto.LivenessAuditLog;
            entity.PhotoUrl = request.Dto.PhotoUrl;
        }

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
