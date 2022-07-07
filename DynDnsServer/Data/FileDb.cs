using DynDnsServer.Data.Model;
using Newtonsoft.Json;

namespace DynDnsServer.Data;

public static class FileDb
{
    private const string DbFileName = "app.db";

    private record DatabaseWrapper(AppSettings AppSettings, List<ManagedDnsRecord> Records);

    private static DatabaseWrapper? _databaseWrapper = null;
    private static DateTime         _lastReadTime    = DateTime.Now;

    private static readonly object _ioLock = new object();

    public static AppSettings AppSettings
    {
        get
        {
            Read();
            if (_databaseWrapper == null) throw new Exception("Database not found");
            return _databaseWrapper.AppSettings;
        }
    }

    public static List<ManagedDnsRecord> Records
    {
        get
        {
            Read();
            if (_databaseWrapper == null) throw new Exception("Database not found");
            return _databaseWrapper.Records;
        }
    }

    private static void Read()
    {
        if (_databaseWrapper != null && _lastReadTime - DateTime.Now < TimeSpan.FromMinutes(1))
            return;

        string filePath = Path.Combine(Environment.CurrentDirectory, DbFileName);

        if (!File.Exists(filePath))
        {
            _databaseWrapper = new DatabaseWrapper(new AppSettings(), new List<ManagedDnsRecord>());
            Save();
            return;
        }

        _lastReadTime = DateTime.Now;

        lock (_ioLock)
        {
            _databaseWrapper = JsonConvert.DeserializeObject<DatabaseWrapper>(File.ReadAllText(filePath));
        }
    }

    public static void Save()
    {
        string filePath = Path.Combine(Environment.CurrentDirectory, DbFileName);
        lock (_ioLock)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(_databaseWrapper, Formatting.Indented));
        }
    }
}
