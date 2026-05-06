using AutoMapper;
using MediatR;
using POS.Application.DTOs;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using Entity = POS.Domain.Entities.TillSession;

namespace POS.Application.Commands.TillSession.Create;

public class CreateTillSessionCommandHandler : IRequestHandler<CreateTillSessionCommand, TillSessionDto>
{
    private readonly ITillSessionRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly ITerminalRepository _terminalRepository;

    public CreateTillSessionCommandHandler(
        ITillSessionRepository repository, 
        IUnitOfWork uow, 
        IMapper mapper, 
        ITenantContext tenantContext,
        ITerminalRepository terminalRepository)
    {
        _repository = repository;
        _uow = uow;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _terminalRepository = terminalRepository;
    }

    public async Task<TillSessionDto> Handle(CreateTillSessionCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.Dto);

        if (_tenantContext.UserId == null)
        {
            throw new UnauthorizedAccessException("Current user is not authenticated or StaffId is missing from token.");
        }
        
        entity.StaffId = _tenantContext.UserId.Value;

        // Terminal validation to prevent FK constraint exception
        var terminal = await _terminalRepository.GetByIdAsync(entity.TerminalId);
        if (terminal == null)
        {
            // Fallback for dev: if the terminal doesn't exist, try to use the first available terminal
            var allTerminals = await _terminalRepository.GetAllAsync();
            var firstTerminal = allTerminals.FirstOrDefault();
            
            if (firstTerminal == null)
            {
                throw new InvalidOperationException("No terminals exist in the system. Please create a terminal first.");
            }
            
            entity.TerminalId = firstTerminal.Id;
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TillSessionDto>(entity);
    }
}
