using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Staff.GetPaged;

public class GetStaffsPagedQueryHandler : IRequestHandler<GetStaffsPagedQuery, PagedResult<StaffDto>>
{
    private readonly IStaffRepository _repository;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IMapper _mapper;

    public GetStaffsPagedQueryHandler(
        IStaffRepository repository, 
        ITransactionRepository transactionRepo,
        IMapper mapper)
    {
        _repository = repository;
        _transactionRepo = transactionRepo;
        _mapper = mapper;
    }

    public async Task<PagedResult<StaffDto>> Handle(GetStaffsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        var dtos = _mapper.Map<List<StaffDto>>(pagedEntities.Items);

        var today = DateTimeOffset.UtcNow.Date;

        foreach (var dto in dtos)
        {
            var transactions = _transactionRepo.GetQueryable()
                .Where(t => t.CashierId == dto.Id && t.Status == POS.Domain.Enums.TransactionStatus.Completed);

            dto.LifetimeRevenue = await transactions.SumAsync(t => t.GrandTotal, cancellationToken);
            dto.TodayRevenue = await transactions
                .Where(t => t.CompletedAt >= today)
                .SumAsync(t => t.GrandTotal, cancellationToken);
        }
        
        return new PagedResult<StaffDto>
        {
            Items = dtos,
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
