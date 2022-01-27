using System.Data;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using ErogeHelper.Model.DataModel.Tables;
using ErogeHelper.Model.Repositories.Interface;
using Microsoft.Data.Sqlite;

namespace ErogeHelper.Model.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly string _connectString;
    
    public CommentRepository(string connectString)
    {
        _connectString = connectString;
    }

    private IDbConnection GetOpenConnection()
    {
        var connection = new SqliteConnection(_connectString);
        connection.Open();
        return connection;
    }

    private const string QueryCommentByMD5Sql = "SELECT * FROM Comment WHERE GameMd5 = @a;";

    public IEnumerable<CommentTable> GetAllCommentOfGame(string md5)
    {
        using var connection = GetOpenConnection();
        return connection.Query<CommentTable>(QueryCommentByMD5Sql, new { a = md5 });
    }

    private const string QueryCommentByHashSql = "SELECT * FROM Comment WHERE Hash = @a;";

    //I'm first string
    //second
    //here im
    public IEnumerable<string> GetAllCommentByHash(long hash)
    {
        using var connection = GetOpenConnection();
        return connection.Query<CommentTable>(QueryCommentByHashSql, new { a = hash }).Select(x => x.UserComment);
    }
}
