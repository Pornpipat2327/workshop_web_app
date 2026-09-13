namespace TodoApi.Dtos;

public record TodoUpdateDto(
    string Title,
    bool IsCompleted
);

