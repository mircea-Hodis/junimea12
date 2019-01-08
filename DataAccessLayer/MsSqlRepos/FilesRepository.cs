using System.Collections.Generic;
using System.Threading.Tasks;
using AuthWebApi.IRepository;
using Dapper;
using DataAccessLayer.IRepository;
using DataModelLayer.Models.Posts;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace DataAccessLayer.MsSqlRepos
{
    public class FilesRepository : IFilesRepository
    {
        private readonly string _mysqlConnectionString; 
        public FilesRepository(IConfiguration configurationService)
        {
            _mysqlConnectionString = configurationService.GetConnectionString("MysqlConnection");
        }

        public async Task<List<PostFiles>> AddPostImagesAsync(List<PostFiles> postFiles)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                foreach (var file in postFiles)
                {
                    file.Id = await connection.QuerySingleAsync<int>(
                            $@"INSERT INTO juniro.PostFiles(
                                PostId, 
                                Url)
                            VALUES (
                                @{nameof(file.PostId)},
                                @{nameof(file.Url)});
                            SELECT LAST_INSERT_ID();"
                                    , file);
                }
            }
            return postFiles;
        }

        public async Task<List<PostFiles>> UpdatePostImagesAsync(List<PostFiles> postFiles, int postId)
        {
            using (var connection = new MySqlConnection(_mysqlConnectionString))
            {
                await connection.ExecuteAsync($@"DELETE FROM juniro.PostFiles WHERE PostId = {postId}");

                foreach (var file in postFiles)
                {
                    file.Id = await connection.QuerySingleAsync<int>(
                        $@"INSERT INTO juniro.PostFiles(
                                PostId, 
                                Url)
                            VALUES (
                                @{nameof(file.PostId)},
                                @{nameof(file.Url)});
                            SELECT LAST_INSERT_ID();"
                        , file);
                }
            }

            return postFiles;
        }
    }
}
