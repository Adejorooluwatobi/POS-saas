using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace POS.Application.Commands.Transaction.SyncOfflineTransactions;

public class SyncOfflineTransactionsCommandHandler : IRequestHandler<SyncOfflineTransactionsCommand, SyncOfflineTransactionsResult>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public SyncOfflineTransactionsCommandHandler(
        ITransactionRepository transactionRepository,
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _transactionRepository = transactionRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<SyncOfflineTransactionsResult> Handle(SyncOfflineTransactionsCommand request, CancellationToken cancellationToken)
    {
        var result = new SyncOfflineTransactionsResult();
        
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant is required.");

        try
        {
            var incomingIds = request.Transactions.Select(t => t.Id).ToList();
            
            var existingIds = await _transactionRepository.GetQueryable()
                .Where(t => incomingIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            foreach (var dto in request.Transactions)
            {
                if (existingIds.Contains(dto.Id))
                {
                    continue; // Skip, already synced
                }

                try
                {
                    var newTransaction = new POS.Domain.Entities.Transaction
                    {
                        Id = dto.Id,
                        ReceiptNumber = dto.ReceiptNumber,
                        SessionId = dto.SessionId,
                        StoreId = dto.StoreId,
                        CashierId = dto.CashierId,
                        CustomerId = dto.CustomerId,
                        Status = POS.Domain.Enums.TransactionStatus.Completed,
                        Subtotal = dto.Subtotal,
                        DiscountTotal = dto.DiscountTotal,
                        TaxTotal = dto.TaxTotal,
                        GrandTotal = dto.GrandTotal,
                        AmountPaid = dto.AmountPaid,
                        ChangeGiven = dto.ChangeGiven,
                        CreatedAt = dto.CreatedAt,
                        CompletedAt = dto.CompletedAt ?? dto.CreatedAt,
                        Items = new List<TransactionItem>(),
                        Payments = new List<POS.Domain.Entities.Payment>()
                    };

                    foreach (var itemDto in dto.Items)
                    {
                        var newItem = new TransactionItem
                        {
                            Id = itemDto.Id,
                            TransactionId = newTransaction.Id,
                            VariantId = itemDto.VariantId,
                            ProductName = itemDto.ProductName,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            OriginalPrice = itemDto.OriginalPrice,
                            UnitCost = itemDto.UnitCost,
                            DiscountAmount = itemDto.DiscountAmount,
                            TaxRate = itemDto.TaxRate,
                            TaxAmount = itemDto.TaxAmount,
                            LineTotal = itemDto.LineTotal
                        };
                        newTransaction.Items.Add(newItem);

                        if (itemDto.VariantId.HasValue)
                        {
                            var inventory = await _inventoryRepository.GetQueryable()
                                .FirstOrDefaultAsync(i => i.VariantId == itemDto.VariantId.Value && i.StoreId == dto.StoreId, cancellationToken);
                            
                            if (inventory != null)
                            {
                                inventory.QuantityOnHand -= (int)itemDto.Quantity;
                                _inventoryRepository.Update(inventory);
                            }
                            else
                            {
                                var newInventory = new POS.Domain.Entities.Inventory
                                {
                                    TenantId = tenantId,
                                    StoreId = dto.StoreId,
                                    VariantId = itemDto.VariantId.Value,
                                    QuantityOnHand = -(int)itemDto.Quantity
                                };
                                await _inventoryRepository.AddAsync(newInventory);
                            }
                        }
                    }

                    foreach (var paymentDto in dto.Payments)
                    {
                        var newPayment = new POS.Domain.Entities.Payment
                        {
                            Id = paymentDto.Id,
                            TransactionId = newTransaction.Id,
                            Method = paymentDto.Method,
                            Amount = paymentDto.Amount,
                            AmountTendered = paymentDto.AmountTendered,
                            ChangeGiven = paymentDto.ChangeGiven,
                            Status = paymentDto.Status,
                            GatewayRef = paymentDto.GatewayRef,
                            ProcessedAt = paymentDto.ProcessedAt ?? dto.CompletedAt
                        };
                        newTransaction.Payments.Add(newPayment);
                    }

                    await _transactionRepository.AddAsync(newTransaction);
                    result.SyncedCount++;
                }
                catch (Exception)
                {
                    result.FailedTransactionIds.Add(dto.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            result.Success = result.FailedTransactionIds.Count == 0;
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
