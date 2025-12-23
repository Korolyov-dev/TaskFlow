// TaskFlow.Application/Services/BoardService.cs
using FluentValidation;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Board;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Common;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IColumnRepository _columnRepository;
    private readonly ILabelRepository _labelRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUserRepository _userRepository;

    private readonly IValidator<CreateBoardRequest> _createBoardValidator;
    private readonly IValidator<UpdateBoardRequest> _updateBoardValidator;
    private readonly IValidator<AddMemberRequest> _addMemberValidator;

    public BoardService(
        IBoardRepository boardRepository,
        IColumnRepository columnRepository,
        ILabelRepository labelRepository,
        IActivityLogRepository activityLogRepository,
        IUserRepository userRepository,
        IValidator<CreateBoardRequest> createBoardValidator,
        IValidator<UpdateBoardRequest> updateBoardValidator,
        IValidator<AddMemberRequest> addMemberValidator)
    {
        _boardRepository = boardRepository;
        _columnRepository = columnRepository;
        _labelRepository = labelRepository;
        _activityLogRepository = activityLogRepository;
        _userRepository = userRepository;
        _createBoardValidator = createBoardValidator;
        _updateBoardValidator = updateBoardValidator;
        _addMemberValidator = addMemberValidator;
    }

    public async Task<ValidationResult<Board>> CreateBoardAsync(
        Guid ownerId,
        CreateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        // Валидация запроса
        var validationResult = await _createBoardValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ValidationResult<Board>.Failure(errors);
        }

        // Проверка существования пользователя
        var user = await _userRepository.GetByIdAsync(ownerId, cancellationToken);
        if (user == null)
        {
            return ValidationResult<Board>.Failure(new[] { $"User with ID '{ownerId}' not found" });
        }

        try
        {
            // Создание доски
            var board = Board.Create(
                request.Title.Trim(),
                ownerId,
                request.Description?.Trim(),
                request.Color
            );

            // Сохранение
            var savedBoard = await _boardRepository.AddAsync(board, cancellationToken);

            // Создание колонок по умолчанию
            await CreateDefaultColumnsAsync(savedBoard.Id, cancellationToken);

            // Создание меток по умолчанию
            await CreateDefaultLabelsAsync(savedBoard.Id, cancellationToken);

            // Добавление владельца как участника
            await _boardRepository.AddMemberAsync(savedBoard.Id, ownerId, BoardRole.Owner, cancellationToken);

            // Логирование активности
            await LogBoardCreatedAsync(savedBoard, ownerId, cancellationToken);

            return ValidationResult<Board>.Success(savedBoard);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Board>.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return ValidationResult<Board>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult<Board?>> GetBoardByIdAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        try
        {
            var board = await _boardRepository.GetByIdAsync(boardId, cancellationToken);
            return ValidationResult<Board?>.Success(board);
        }
        catch (Exception ex)
        {
            return ValidationResult<Board?>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult<Board?>> GetBoardWithDetailsAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        try
        {
            var board = await _boardRepository.GetBoardWithDetailsAsync(boardId, cancellationToken);
            return ValidationResult<Board?>.Success(board);
        }
        catch (Exception ex)
        {
            return ValidationResult<Board?>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult<IReadOnlyList<Board>>> GetUserBoardsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var boards = await _boardRepository.GetUserBoardsAsync(userId, cancellationToken);
            return ValidationResult<IReadOnlyList<Board>>.Success(boards);
        }
        catch (Exception ex)
        {
            return ValidationResult<IReadOnlyList<Board>>.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult> UpdateBoardAsync(
        Guid boardId,
        UpdateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        // Валидация
        var validationResult = await _updateBoardValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ValidationResult.Failure(errors);
        }

        try
        {
            var board = await _boardRepository.GetByIdAsync(boardId, cancellationToken);
            if (board == null)
            {
                return ValidationResult.Failure(new[] { $"Board with ID '{boardId}' not found" });
            }

            board.UpdateTitle(request.Title.Trim());
            board.UpdateDescription(request.Description?.Trim());
            board.UpdateColor(request.Color);

            await _boardRepository.UpdateAsync(board, cancellationToken);

            return ValidationResult.Success();
        }
        catch (DomainException ex)
        {
            return ValidationResult.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure(new[] { "An unexpected error occurred" });
        }
    }

    public async Task<ValidationResult> AddMemberAsync(
        Guid boardId,
        AddMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        // Валидация
        var validationResult = await _addMemberValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ValidationResult.Failure(errors);
        }

        try
        {
            // Проверка существования доски
            var board = await _boardRepository.GetByIdAsync(boardId, cancellationToken);
            if (board == null)
            {
                return ValidationResult.Failure(new[] { $"Board with ID '{boardId}' not found" });
            }

            // Проверка существования пользователя
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return ValidationResult.Failure(new[] { $"User with ID '{request.UserId}' not found" });
            }

            // Проверка, не является ли пользователь уже участником
            if (await _boardRepository.IsUserBoardMemberAsync(boardId, request.UserId, cancellationToken))
            {
                return ValidationResult.Failure(new[] { $"User '{request.UserId}' is already a member" });
            }

            await _boardRepository.AddMemberAsync(boardId, request.UserId, request.Role, cancellationToken);

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure(new[] { "An unexpected error occurred" });
        }
    }

    // Вспомогательные методы (остаются без изменений)
    private async Task CreateDefaultColumnsAsync(Guid boardId, CancellationToken cancellationToken)
    {
        var defaultColumns = new[]
        {
            Column.Create("To Do", boardId, 0),
            Column.Create("In Progress", boardId, 1),
            Column.Create("Done", boardId, 2)
        };

        foreach (var column in defaultColumns)
        {
            await _columnRepository.AddAsync(column, cancellationToken);
        }
    }

    private async Task CreateDefaultLabelsAsync(Guid boardId, CancellationToken cancellationToken)
    {
        var defaultLabels = Label.CreateDefaultLabels(boardId);

        foreach (var label in defaultLabels)
        {
            await _labelRepository.AddAsync(label, cancellationToken);
        }
    }

    private async Task LogBoardCreatedAsync(Board board, Guid userId, CancellationToken cancellationToken)
    {
        var activity = ActivityLog.ForTask(
            boardId: board.Id,
            userId: userId,
            activityType: ActivityType.BoardCreated,
            description: $"created board '{board.Title}'",
            taskId: board.Id
        );

        await _activityLogRepository.AddAsync(activity, cancellationToken);
    }

    // Остальные методы (ToggleFavoriteAsync, DeleteBoardAsync, RemoveMemberAsync, IsUserMemberAsync)
    // реализуются аналогично с валидацией и ValidationResult
}