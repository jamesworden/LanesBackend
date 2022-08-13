namespace LanesBackend
{
    public interface ICacheService
    {
        public Task<string> GetValue(string key);

        public Task SetValue(string key, string value);
    }
}
