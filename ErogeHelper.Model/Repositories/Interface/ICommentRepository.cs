using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Model.Repositories.Interface;

public interface ICommentRepository
{
    IEnumerable<string> GetAllCommentByHash(long hash);
}
