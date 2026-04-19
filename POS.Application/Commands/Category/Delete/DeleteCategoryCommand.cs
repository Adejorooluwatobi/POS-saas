using System;
using MediatR;

namespace POS.Application.Commands.Category.Delete;

public record DeleteCategoryCommand(Guid Id) : IRequest;
