namespace System.IO
{
    public class TemporaryFile : IDisposable
    {
        private bool _IsDisposed;
        private bool _Keep;
        private string _Path;

        public TemporaryFile()
            : this(false)
        {
        }

        public TemporaryFile(bool shortLived)
        {
            _Path = CreateTemporaryFile(shortLived);
        }

        public string Path
        {
            get { return _Path; }
        }

        public bool Keep
        {
            get { return _Keep; }
            set { _Keep = value; }
        }

        ~TemporaryFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_IsDisposed)
            {
                _IsDisposed = true;

                if (!_Keep)
                {
                    TryDelete();
                }
            }
        }

        private void TryDelete()
        {
            try
            {
                File.Delete(_Path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static string CreateTemporaryFile(bool shortLived)
        {
            string temporaryFile = System.IO.Path.GetTempFileName();

            if (shortLived)
            {
                // Set the temporary attribute, meaning the file will live in memory and will not be written to disk 
                File.SetAttributes(temporaryFile, File.GetAttributes(temporaryFile) | FileAttributes.Temporary);
            }

            return temporaryFile;
        }
    }
}