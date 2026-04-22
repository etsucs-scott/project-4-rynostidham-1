using Xunit;
using FinalProject.Core;

public class FileStorageTests
{
    [Fact]
    public void SaveLoad_Works()
    {
        var file = Path.GetTempFileName();
        var storage = new FileStorageService(file);

        storage.Save(new GameSave { BestWave = 7 });
        var loaded = storage.Load();

        Assert.Equal(7, loaded.BestWave);
    }
}
