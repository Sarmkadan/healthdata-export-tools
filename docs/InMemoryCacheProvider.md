# InMemoryCacheProvider

A lightweight, in-memory implementation of a key-value cache for temporary storage of application data. It provides asynchronous methods for common cache operations and exposes statistics about cache usage. This provider is intended for scenarios where persistence is not required and where cache entries can safely reside in memory for the lifetime of the application or until explicitly removed.

## API

### `InMemoryCacheProvider`
Initializes a new instance of the `InMemoryCacheProvider` class. The cache is created with default settings and is not thread-safe by default; concurrent access requires external synchronization.

### `Task<T?> GetAsync<T>(string key)`
Retrieves the value associated with the specified `key` from the cache.

- **Parameters**
  - `key`: The unique identifier for the cached value.
- **Return value**
  - A `Task` resolving to the cached value of type `T`, or `null` if the key does not exist or the value is not of type `T`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)`
Stores a value in the cache with an optional expiration time.

- **Parameters**
  - `key`: The unique identifier for the value.
  - `value`: The value to cache.
  - `expiry`: Optional duration after which the entry expires. If `null`, the entry does not expire.
- **Return value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentNullException` if `value` is `null` and `T` is a reference type.

### `Task RemoveAsync(string key)`
Removes the cached entry associated with the specified `key`.

- **Parameters**
  - `key`: The unique identifier for the cached value.
- **Return value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `Task<bool> ExistsAsync(string key)`
Determines whether a cached entry with the specified `key` exists.

- **Parameters**
  - `key`: The unique identifier for the cached value.
- **Return value**
  - A `Task<bool>` resolving to `true` if the key exists; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `Task ClearAsync()`
Removes all entries from the cache.

- **Return value**
  - A `Task` representing the asynchronous operation.

### `Task<List<string>> GetKeysAsync()`
Retrieves a list of all keys currently stored in the cache.

- **Return value**
  - A `Task<List<string>>` resolving to a list of cache keys. The list is unordered and may include expired entries that are lazily cleaned up on access.
- **Exceptions**
  - Never throws.

### `Task<CacheStats> GetStatsAsync()`
Retrieves statistics about the current state of the cache.

- **Return value**
  - A `Task<CacheStats>` resolving to a `CacheStats` object containing counts of total entries, hits, misses, and memory usage.
- **Exceptions**
  - Never throws.

### `void Dispose()`
Releases all resources used by the `InMemoryCacheProvider` instance. After disposal, the instance should not be used.

- **Exceptions**
  - Never throws.

## Usage

### Basic Usage
