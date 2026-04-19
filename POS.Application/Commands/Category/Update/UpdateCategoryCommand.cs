using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Category.Update;

public record UpdateCategoryCommand(Guid Id, UpdateCategoryDto Dto) : IRequest;
