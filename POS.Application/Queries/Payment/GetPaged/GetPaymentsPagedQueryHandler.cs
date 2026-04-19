using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Common;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Payment.GetPaged;

public class GetPaymentsPagedQueryHandler : IRequestHandler<GetPaymentsPagedQuery, PagedResult<PaymentDto>>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetPaymentsPagedQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
        
        return new PagedResult<PaymentDto>
        {
            Items = _mapper.Map<IEnumerable<PaymentDto>>(pagedEntities.Items),
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };
    }
}
