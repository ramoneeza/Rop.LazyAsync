using System.Runtime.CompilerServices;

namespace Rop.LazyAsync
{
    public class LazyAsync<T> : IAsyncDisposable
    {
        private readonly Func<Task<T>> _factory;
        private Lazy<Task<T>> _lazy;
        public LazyAsync(Func<Task<T>> factory)
        {
            _factory = factory;
            _lazy = new Lazy<Task<T>>(_factory);
        }
        public LazyAsync(Func<T> factory) : this(() => Task.Run(factory)) { }
        public bool IsValueCreated => _lazy.IsValueCreated;
        public Task<T> Value => _lazy.Value;

        public async ValueTask Reset()
        {
            if (this.IsValueCreated)
            {
                await DisposeLazy();
                _lazy = new Lazy<Task<T>>(_factory);
            }
        }

        private async ValueTask DisposeLazy()
        {
            if (!this.IsValueCreated) return;

            var t = await this;
            switch (t)
            {
                case IAsyncDisposable ad:
                    await ad.DisposeAsync();
                    break;
                case IDisposable d:
                    d.Dispose();
                    break;
            }

        }

        public async ValueTask DisposeAsync()
        {
            await DisposeLazy();
        }
        public TaskAwaiter<T> GetAwaiter() => Value.GetAwaiter();
    }
}