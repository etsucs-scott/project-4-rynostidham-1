using System.Text.Json;

namespace FinalProject.Core;

public class FileStorageService
{
    private readonly string _path;

    public FileStorageService(string path)
    {
        _path = path;
    }

    public GameSave Load()
    {
        try
        {
            if (!File.Exists(_path))
                return new GameSave();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<GameSave>(json) ?? new GameSave();
        }
        catch
        {
            return new GameSave();
        }
    }

    public void Save(GameSave save)
    {
        try
        {
            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, json);
        }
        catch
        {
            // swallow errors safely
        }
    }
}
