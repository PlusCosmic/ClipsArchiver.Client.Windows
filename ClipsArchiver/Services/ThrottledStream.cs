namespace ClipsArchiver.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledStream(Stream @in, int throttleKb) : Stream
{
    private readonly int _throttle = throttleKb * 1024;
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    private long _totalBytesRead;

    public override bool CanRead => @in.CanRead;

    public override bool CanSeek => @in.CanSeek;

    public override bool CanWrite => false;

    public override void Flush()
    {
    }

    public override long Length => @in.Length;

    public override long Position
    {
        get => @in.Position;
        set => @in.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var newcount = GetBytesToReturn(count);
        int read = @in.Read(buffer, offset, newcount);
        Interlocked.Add(ref _totalBytesRead, read);
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return @in.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
    }

    int GetBytesToReturn(int count)
    {
        return GetBytesToReturnAsync(count).Result;
    }

    async Task<int> GetBytesToReturnAsync(int count)
    {
        if (_throttle <= 0)
            return count;

        long canSend = (long)(_watch.ElapsedMilliseconds * (_throttle / 1000.0));

        int diff = (int)(canSend - _totalBytesRead);

        if (diff <= 0)
        {
            var waitInSec = ((diff * -1.0) / (_throttle));

            await Task.Delay((int)(waitInSec * 1000)).ConfigureAwait(false);
        }

        if (diff >= count) return count;

        return diff > 0 ? diff : Math.Min(1024 * 8, count);
    }
}