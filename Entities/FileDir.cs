namespace GoBackendProjectStarter.Entities;

internal class FileDir
{
    public String PathName { get; set; } = null!;

    public IEnumerable<FileDir> FolderChildren { get; set; } = Enumerable.Empty<FileDir>();

    public String? FileText { get; set; }
}