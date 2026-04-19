using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Payment.GetById;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetPaymentByIdQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        return entity != null ? _mapper.Map<PaymentDto>(entity) : null;
    }
}
