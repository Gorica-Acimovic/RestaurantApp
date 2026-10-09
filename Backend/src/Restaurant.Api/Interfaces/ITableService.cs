using Restaurant.Api.DTOs;

namespace Restaurant.Api.Interfaces
{
    public interface ITableService
    {
        Task<List<TableDto>> GetAllTablesAsync();
        Task<(TableDto? Table, string? Error)> CreateTableAsync(SaveTableRequest request);
        Task<(bool Found, string? Error)> UpdateTableAsync(int id, SaveTableRequest request);
        Task<bool> UpdatePositionOfTableAsync(int id, UpdateTablePositionRequest request);
        Task<bool> DeleteTableAsync(int id);
        Task<bool> UpdateStatusAsync(int id, UpdateTableStatusRequest request);
    }
}
