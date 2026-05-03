using AutoMapper;
using MediatR;
using POS.Application.DTOs.StockRequisition;
using POS.Domain.Repositories;

namespace POS.Application.Queries.StockRequisition.GetById;

public class GetStockRequisitionByIdQueryHandler : IRequestHandler<GetStockRequisitionByIdQuery, StockRequisitionDto>
{
    private readonly IStockRequisitionRepository _repository;
    private readonly IMapper _mapper;

    public GetStockRequisitionByIdQueryHandler(IStockRequisitionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StockRequisitionDto> Handle(GetStockRequisitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Stock requisition not found.");
            
        return _mapper.Map<StockRequisitionDto>(entity);
    }
}
