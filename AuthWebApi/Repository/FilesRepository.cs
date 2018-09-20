using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AuthWebApi.IRepository;
using AuthWebApi.Models.Posts;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace AuthWebApi.Repository
{
    public class FilesRepository : IFilesRepository
    {
        private readonly string _connectionString;
        public FilesRepository(IConfiguration configurationService)
        {
            _connectionString = configurationService.GetConnectionString("DefaultConnection");
        }

        public async Task<List<PostFiles>> AddPostImagesAsync(List<PostFiles> postFiles)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                foreach (var file in postFiles)
                {
                    file.Id = await connection.QuerySingleAsync<int>(
                            $@"INSERT INTO [dbo].[PostFiles](
                                [PostId], 
                                [Url])
                            VALUES (
                                @{nameof(file.PostId)},
                                @{nameof(file.Url)} 
                                );SELECT CAST(SCOPE_IDENTITY() as int)"
                                    , file);
                }
            }
            return postFiles;
        }
    }
}
