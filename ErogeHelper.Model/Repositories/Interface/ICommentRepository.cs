using ErogeHelper.Model.DataModel.Tables;

namespace ErogeHelper.Model.Repositories.Interface;

public interface ICommentRepository
{
    IEnumerable<CommentTable> GetAllCommentOfGame(string md5);

    IEnumerable<string> GetAllCommentByHash(long hash);
}
