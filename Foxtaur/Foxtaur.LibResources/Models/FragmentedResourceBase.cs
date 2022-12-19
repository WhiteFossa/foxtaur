using Foxtaur.LibResources.Constants;
using NLog;
using ZstdNet;

namespace Foxtaur.LibResources.Models;

/// <summary>
/// Delegate for OnLoaded() event
/// </summary>
public delegate void OnFragmentedResourceLoaded(FragmentedResourceBase fragment);

/// <summary>
/// Fragmented resource base class
/// Fragmented resource is a resource, limited by geocoordinates
/// </summary>
public abstract class FragmentedResourceBase
{
    /// <summary>
    /// North border of a fragment
    /// </summary>
    public double NorthLat { get; private set; }

    /// <summary>
    /// South border of fragment
    /// </summary>
    public double SouthLat { get; private set; }

    /// <summary>
    /// Western border (fragment can't stretch over 180 / -180 line)
    /// </summary>
    public double WestLon { get; private set; }

    /// <summary>
    /// Eastern border
    /// </summary>
    public double EastLon { get; private set; }

    /// <summary>
    /// Unique resource name
    /// </summary>
    public string ResourceName { get; private set; }

    /// <summary>
    /// If true, then RemotePath points to a local file, so no need to download anything
    /// </summary>
    public bool IsLocal { get; private set; }

    /// <summary>
    /// Call this when resource load is completed
    /// </summary>
    protected OnFragmentedResourceLoaded OnLoad;

    /// <summary>
    /// Semaphore for limit the number of active downloading threads
    /// </summary>
    protected static Semaphore DownloadThreadsLimiter = new Semaphore(ResourcesConstants.MaxActiveDownloadingThreads, ResourcesConstants.MaxActiveDownloadingThreads); 
    
    private Logger _logger = LogManager.GetCurrentClassLogger();

    private static Mutex _downloadMutex = new Mutex();

    public FragmentedResourceBase(double northLat,
        double southLat,
        double westLon,
        double eastLon,
        string resourceName,
        bool isLocal)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            throw new ArgumentException(nameof(resourceName));
        }

        if (northLat <= southLat)
        {
            throw new ArgumentException("NorthLat must be norther than SouthLat");
        }

        if (westLon <= eastLon)
        {
            throw new ArgumentException("WestLon must be wester than EastLon");
        }

        NorthLat = northLat;
        SouthLat = southLat;
        WestLon = westLon;
        EastLon = eastLon;
        ResourceName = resourceName;
        IsLocal = isLocal;
    }

    /// <summary>
    /// After download resource can be found here
    /// </summary>
    /// <returns></returns>
    public virtual string GetLocalPath()
    {
        if (IsLocal)
        {
            return ResourceName; // For local resources local path == remote path
        }

        return GetResourceLocalPath(ResourceName);
    }

    /// <summary>
    /// Is given coordinates hit the resource
    /// </summary>
    public bool IsHit(double lat, double lon)
    {
        return lat >= SouthLat && lat <= NorthLat && lon >= EastLon && lon <= WestLon;
    }

    /// <summary>
    /// Download resource
    /// </summary>
    public abstract void Download(OnFragmentedResourceLoaded onLoad);

    /// <summary>
    /// Load resource as a stream from a relative URL
    /// </summary>
    protected MemoryStream LoadFromUrl(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            throw new ArgumentException(nameof(relativeUrl));
        }

        var uri = ResourcesConstants.ResourcesBaseUrl + relativeUrl;
        Uri uriResult;
        if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
        {
            throw new ArgumentException(nameof(relativeUrl));
        }

        try
        {
            _logger.Info($"Waiting for download from {uriResult}");

            DownloadThreadsLimiter.WaitOne();

            _logger.Info($"Downloading from {uriResult}");

            _downloadMutex.WaitOne();

            try
            {
                var httpClient = new HttpClient();
                var webRequest = new HttpRequestMessage(HttpMethod.Get, uriResult);
                var downloadStream = httpClient.Send(webRequest).Content.ReadAsStream();

                var resultStream = new MemoryStream();
                downloadStream.CopyTo(resultStream);
                downloadStream.Dispose();
                
                return resultStream;
            }
            finally
            {
                _downloadMutex.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
            throw;
        }
        finally
        {
            DownloadThreadsLimiter.Release();
        }
    }

    protected void LoadFromUrlToFile(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            throw new ArgumentException(nameof(relativeUrl));
        }

        using (var downloadStream = LoadFromUrl(relativeUrl))
        {
            var localPath = GetResourceLocalPath(relativeUrl);
            
            _logger.Info($"Saving { relativeUrl } to { localPath }");
            SaveStreamAsFile(downloadStream, localPath);
        }
    }

    /// <summary>
    /// Return path, where downloaded resource have to be stored
    /// </summary>
    protected string GetResourceLocalPath(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            throw new ArgumentException(nameof(relativeUrl));
        }
        
        return ResourcesConstants.DownloadedDirectory + relativeUrl;
    }

    /// <summary>
    /// Save stream as a file
    /// </summary>
    protected void SaveStreamAsFile(MemoryStream stream, string path)
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(nameof(path));
        }
        
        // Do target directory exist?
        var targetDirectory = Path.GetDirectoryName(path);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        
        using (var fileStream = File.Create(path))
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }
    }
    
    /// <summary>
    /// Load ZSTD file to stream
    /// </summary>
    protected Stream LoadZstdFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException(nameof(path));
        }

        return new DecompressionStream(File.OpenRead(path));
    }
}