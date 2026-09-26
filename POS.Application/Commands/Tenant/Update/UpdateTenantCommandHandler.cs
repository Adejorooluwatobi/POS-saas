using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Tenant.Update;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateTenantCommandHandler(ITenantRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Tenant {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Dto.BusinessName))
        {
            entity.BusinessName = request.Dto.BusinessName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.ContactEmail))
        {
            var cleanEmail = request.Dto.ContactEmail.Trim().ToLowerInvariant();
            if (cleanEmail != entity.ContactEmail.ToLowerInvariant())
            {
                var emailInUse = await _repository.GetQueryable()
                    .AnyAsync(t => t.ContactEmail.ToLower() == cleanEmail && t.Id != request.Id, cancellationToken);
                if (emailInUse)
                {
                    throw new InvalidOperationException($"Contact email '{cleanEmail}' is already used by another tenant.");
                }
                entity.ContactEmail = cleanEmail;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.Slug))
        {
            var cleanSlug = request.Dto.Slug.Trim().ToLowerInvariant();
            if (cleanSlug != entity.Slug.ToLowerInvariant())
            {
                var slugInUse = await _repository.GetQueryable()
                    .AnyAsync(t => t.Slug.ToLower() == cleanSlug && t.Id != request.Id, cancellationToken);
                if (slugInUse)
                {
                    throw new InvalidOperationException($"Tenant slug '{cleanSlug}' is already in use.");
                }
                entity.Slug = cleanSlug;
            }
        }

        entity.ContactPhone = request.Dto.ContactPhone?.Trim();

        if (!string.IsNullOrWhiteSpace(request.Dto.Country))
        {
            entity.Country = request.Dto.Country.Trim();
        }

        if (request.Dto.LogoUrl != null)
        {
            entity.LogoUrl = request.Dto.LogoUrl.Trim();
        }

        entity.IsActive = request.Dto.IsActive;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

