using ClipsArchiver.Entities;
using SQLite;

namespace ClipsArchiver.Services;

public static class LocalDbService
{
    private static readonly string _dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                                            @"\ClipsArchiver\clipsArchiver.db";

    private static SQLiteConnection _dbConnection = new SQLiteConnection(_dbPath);
    private static bool _initialised = false;

    private static void Initialise()
    {
        _dbConnection.CreateTable<LocalClipInfo>();
        _initialised = true;
    }
    
    public static LocalClipInfo GetInfoForClipId(int clipId)
    {
        if (!_initialised)
        {
            Initialise();
        }

        var clipInfo = _dbConnection.Table<LocalClipInfo>().FirstOrDefault(x => x.ClipId == clipId);
        if (clipInfo != null) return clipInfo;
        
        LocalClipInfo info = new()
        {
            ClipId = clipId,
            Watched = false
        };
        SetInfoForClipId(info);
        clipInfo = info;

        return clipInfo;
    }

    public static void SetInfoForClipId(LocalClipInfo clipInfo)
    {
        if (!_initialised)
        {
            Initialise();
        }

        if (_dbConnection.Table<LocalClipInfo>().Count(x => x.ClipId == clipInfo.ClipId) > 0)
        {
            _dbConnection.Update(clipInfo);
        }
        else
        {
            _dbConnection.Insert(clipInfo);
        }
    }
    
    public static LocalClipInfo GetInfoForFileName(string fileName)
    {
        if (!_initialised)
        {
            Initialise();
        }

        var clipInfo = _dbConnection.Table<LocalClipInfo>().FirstOrDefault(x => x.FileName == fileName);
        if (clipInfo != null) return clipInfo;
        
        LocalClipInfo info = new()
        {
            FileName = fileName,
            Watched = false
        };
        SetInfoForClipId(info);
        clipInfo = info;

        return clipInfo;
    }
    
    public static void SetInfoForFileName(LocalClipInfo clipInfo)
    {
        if (!_initialised)
        {
            Initialise();
        }

        if (_dbConnection.Table<LocalClipInfo>().Count(x => x.FileName == clipInfo.FileName) > 0)
        {
            _dbConnection.Update(clipInfo);
        }
        else
        {
            _dbConnection.Insert(clipInfo);
        }
    }
    
    public static List<LocalClipInfo> GetAllClipInfo()
    {
        if (!_initialised)
        {
            Initialise();
        }

        return _dbConnection.Table<LocalClipInfo>().ToList();
    }
}