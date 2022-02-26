namespace ErogeHelper.Shared.Entities;

public record CloudSaveDataTerm(
    string FolderName,
    string LocalPath,
    DateTime LastTimeModified,
    string[] ExcludeFiles,
    string PCName,
    string PCId);
