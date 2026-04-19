using AutoMapper;
using MediatR;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Payment.Update;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand>
{
    private readonly IPaymentRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdatePaymentCommandHandler(IPaymentRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"Payment {request.Id} not found.");

        _mapper.Map(request.Dto, entity);

        if (entity.Status == PaymentStatus.Approved && entity.ProcessedAt is null)
            entity.ProcessedAt = DateTimeOffset.UtcNow;

        _repository.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
