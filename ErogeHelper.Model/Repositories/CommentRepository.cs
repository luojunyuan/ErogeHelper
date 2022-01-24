using ErogeHelper.Model.Repositories.Interface;

namespace ErogeHelper.Model.Repositories;

public class CommentRepository : ICommentRepository
{
    public IEnumerable<string> GetAllCommentByHash(long hash)
    {
        if (hash == 7194484186085652533)
        {
            return new List<string>() { "I'm first string", "second", "here im" };
        }

        return Enumerable.Empty<string>();
    }
}
