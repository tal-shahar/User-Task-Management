using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TaskManagementService.Models;

namespace TaskManagementService.Data
{
    public class TaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<List<TaskModel>> GetOverdueTasksAsync(DateTime currentDate)
        {
            var tasks = new List<TaskModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT Id, Title, Description, DueDate, Priority, UserId, UserFullName, UserEmail, CreatedAt, UpdatedAt
                    FROM Tasks
                    WHERE DueDate < @CurrentDate
                    ORDER BY DueDate ASC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrentDate", currentDate);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tasks.Add(new TaskModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                Priority = (Priority)reader.GetInt32(reader.GetOrdinal("Priority")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName")),
                                UserEmail = reader.GetString(reader.GetOrdinal("UserEmail")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) 
                                    ? (DateTime?)null 
                                    : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                            });
                        }
                    }
                }
            }

            return tasks;
        }
    }
}

