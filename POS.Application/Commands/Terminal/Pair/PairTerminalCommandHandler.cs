using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Commands.Terminal.Pair;

public class PairTerminalCommandHandler : IRequestHandler<PairTerminalCommand, PairTerminalResponse>
{
    private readonly ITerminalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PairTerminalCommandHandler(ITerminalRepository repository, IUnitOfWork uow, IMapper mapper)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PairTerminalResponse> Handle(PairTerminalCommand request, CancellationToken cancellationToken)
    {
        var terminal = await _repository.GetByPairingCodeAsync(request.PairingCode);

        if (terminal == null)
        {
            return new PairTerminalResponse { Success = false, Message = "Invalid pairing code." };
        }

        if (terminal.PairingCodeExpiresAt.HasValue && terminal.PairingCodeExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            return new PairTerminalResponse { Success = false, Message = "Pairing code has expired. Please generate a new one in the Admin Portal." };
        }

        // Generate a long-lived device token
        var deviceToken = Guid.NewGuid().ToString("N");
        
        terminal.DeviceToken = deviceToken;
        terminal.PairingCode = null; // Clear the pairing code so it can't be reused
        terminal.PairingCodeExpiresAt = null;
        terminal.Status = Domain.Enums.TerminalStatus.Online; // Mark as online since it just paired

        _repository.Update(terminal);
        await _uow.SaveChangesAsync(cancellationToken);

        return new PairTerminalResponse
        {
            Success = true,
            Message = "Device successfully paired.",
            DeviceToken = deviceToken,
            Terminal = _mapper.Map<TerminalDto>(terminal)
        };
    }
}
