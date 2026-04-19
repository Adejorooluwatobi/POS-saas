using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Transaction.Create;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _repository;
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly IReceiptNumberService _receiptNumberService;

    public CreateTransactionCommandHandler(
        ITransactionRepository repository, IStoreRepository storeRepository,
        IUnitOfWork uow, IMapper mapper,
        ITenantContext tenantContext, IReceiptNumberService receiptNumberService)
    {
        _repository = repository;
        _storeRepository = storeRepository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _receiptNumberService = receiptNumberService;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var store = await _storeRepository.GetByIdAsync(request.Dto.StoreId)
            ?? throw new KeyNotFoundException($"Store {request.Dto.StoreId} not found.");

        var entity = _mapper.Map<POS.Domain.Entities.Transaction>(request.Dto);
        entity.CashierId = _tenantContext.UserId!.Value;
        entity.ReceiptNumber = _receiptNumberService.Generate(store.Code);

        foreach (var itemDto in request.Dto.Items)
        {
            var taxAmount = itemDto.UnitPrice * itemDto.Quantity * itemDto.TaxRate;
            var lineTotal = itemDto.UnitPrice * itemDto.Quantity + taxAmount;

            entity.Items.Add(new TransactionItem
            {
                TransactionId = entity.Id,
                VariantId = itemDto.VariantId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                OriginalPrice = itemDto.UnitPrice,
                TaxRate = itemDto.TaxRate,
                TaxAmount = taxAmount,
                LineTotal = lineTotal
            });
        }

        entity.Subtotal = entity.Items.Sum(i => i.UnitPrice * i.Quantity);
        entity.TaxTotal = entity.Items.Sum(i => i.TaxAmount);
        entity.GrandTotal = entity.Subtotal + entity.TaxTotal - entity.DiscountTotal;

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TransactionDto>(entity);
    }
}
