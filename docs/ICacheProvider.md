# ICacheProvider

`ICacheProvider` is a generic interface that defines a contract for cache implementations. It provides methods and properties to store, retrieve, and manage cached items with metadata such as size, access counts, and expiration times.

## API

### `public int ItemCount`
Gets the current number of items in the cache.
**Returns:** The count of items stored in the cache.
**Throws:** No exceptions are specified for this property.

---

### `public long TotalSize`
Gets the total size, in bytes, of all items in the cache.
**Returns:** The cumulative size of all cached items.
**Throws:** No exceptions are specified for this property.

---
### `public int HitCount`
Gets the total number of cache hits (successful retrievals).
**Returns:** The count of cache hits.
**Throws:** No exceptions are specified for this property.

---
### `public int MissCount`
Gets the total number of cache misses (failed retrievals).
**Returns:** The count of cache misses.
**Throws:** No exceptions are specified for this property.

---
### `public string Key`
Gets the key associated with the current cache entry.
**Returns:** The key string for the entry.
**Throws:** No exceptions are specified for this property.

---
### `public T? Value`
Gets the value associated with the current cache entry.
**Returns:** The cached value of type `T`, or `null` if not set.
**Throws:** No exceptions are specified for this property.

---
### `public DateTime CreatedAt`
Gets the timestamp when the current cache entry was created.
**Returns:** The creation timestamp of the entry.
**Throws:** No exceptions are specified for this property.

---
### `public DateTime? ExpiresAt`
Gets the timestamp when the current cache entry expires, if set.
**Returns:** The expiration timestamp, or `null` if no expiration is set.
**Throws:** No exceptions are specified for this property.

---
### `public int AccessCount`
Gets the total number of times the current cache entry has been accessed.
**Returns:** The access count for the entry.
**Throws:** No exceptions are specified for this property.

---
### `public DateTime? LastAccessAt`
Gets the timestamp of the last access to the current cache entry.
**Returns:** The last access timestamp, or `null` if never accessed.
**Throws:** No exceptions are specified for this property.

## Usage

### Example 1: Basic Cache Operations
```csharp
var cache = new MemoryCacheProvider<string, byte[]>();
var key = "user_123_profile";
var data = Encoding.UTF8.GetBytes("sample data");

// Add to cache
cache.Set(key, data, TimeSpan.FromMinutes(5));

// Retrieve from cache
var cachedData = cache.Get(key);
Console.WriteLine($"Value exists: {cachedData != null}");

// Access metadata
Console.WriteLine($"Item count: {cache.ItemCount}");
Console.WriteLine($"Total size: {cache.TotalSize} bytes");
```

### Example 2: Monitoring Cache Hits and Misses
```csharp
var cache = new MemoryCacheProvider<int, string>();
var provider = (ICacheProvider<string>)cache;

// Simulate cache misses
for (int i = 0; i < 5; i++)
{
    cache.Get($"missing_key_{i}");
}

Console.WriteLine($"Miss count: {provider.MissCount}"); // 5

// Simulate cache hits
cache.Set("valid_key", "value", TimeSpan.FromHours(1));
for (int i = 0; i < 3; i++)
{
    cache.Get("valid_key");
}

Console.WriteLine($"Hit count: {provider.HitCount}"); // 3
Console.WriteLine($"Access count: {cache.AccessCount}"); // 3
```

## Notes

- **Thread Safety:** All properties are read-only and return immutable values (e.g., `int`, `long`, `DateTime`). The underlying cache implementation should ensure thread-safe updates to these values when items are added, accessed, or expired.
- **Null Handling:** `Value`, `ExpiresAt`, and `LastAccessAt` may return `null` if the entry is not set, expired, or never accessed, respectively. Consumers should check for `null` before using these values.
- **Size Calculation:** `TotalSize` should reflect the sum of all cached item sizes in bytes. Implementations must ensure this value is updated atomically with cache modifications.
- **Expiration Edge Cases:** If `ExpiresAt` is set to a past timestamp, the entry should be treated as expired and inaccessible, even if `Value` is still present in memory.
