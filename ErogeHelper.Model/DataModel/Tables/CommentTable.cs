using Dapper.Contrib.Extensions;

namespace ErogeHelper.Model.DataModel.Tables;

[Table("CommentTable")]
public record CommentTable
{
    public long Hash { get; set; }

    public string Text { get; set; } = string.Empty;

    public string UserComment { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    [Key]
    public string CommentId { get; set; } = string.Empty;

    public string GameMd5 { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }
}
