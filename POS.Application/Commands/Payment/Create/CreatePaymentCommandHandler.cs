using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.Payment;

namespace POS.Application.Commands.Payment.Create;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreatePaymentCommandHandler(IPaymentRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);

        if (entity.Method == PaymentMethod.Cash && entity.AmountTendered.HasValue)
            entity.ChangeGiven = entity.AmountTendered.Value - entity.Amount;

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PaymentDto>(entity);
    }
}
