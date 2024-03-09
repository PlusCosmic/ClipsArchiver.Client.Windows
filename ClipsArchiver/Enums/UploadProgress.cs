namespace ClipsArchiver.Enums;

public enum UploadProgress
{
    Ready,
    Uploading,
    PendingTranscode,
    Transcoding,
    Finished,
    FailedExists,
    FailedError
}