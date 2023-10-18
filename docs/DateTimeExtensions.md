# DateTimeExtensions

Provides a collection of static utility methods for common `DateTime` manipulations, comparisons, and formatting. This type simplifies operations such as calculating day boundaries, extracting age, measuring elapsed time, and producing standardized string representations.

## API

### StartOfDay
`public static DateTime StartOfDay(this DateTime date)`

Returns a `DateTime` set to the earliest moment of the given date (midnight, `00:00:00`). The `Kind` property of the original value is preserved.

### EndOfDay
`public static DateTime EndOfDay(this DateTime date)`

Returns a `DateTime` set to the last moment of the given date (`23:59:59.9999999`). The `Kind` property of the original value is preserved.

### StartOfWeek
`public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)`

Returns a `DateTime` representing midnight of the first day of the week containing `date`. The caller specifies which day constitutes the start of the week; the default is Monday.

### EndOfWeek
`public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)`

Returns a `DateTime` representing the last moment of the final day of the week containing `date`, based on the supplied `startOfWeek`.

### StartOfMonth
`public static DateTime StartOfMonth(this DateTime date)`

Returns a `DateTime` set to midnight on the first day of the month that contains `date`.

### EndOfMonth
`public static DateTime EndOfMonth(this DateTime date)`

Returns a `DateTime` set to the last moment of the final day of the month that contains `date`.

### StartOfYear
`public static DateTime StartOfYear(this DateTime date)`

Returns a `DateTime` set to midnight on January 1 of the year that contains `date`.

### EndOfYear
`public static DateTime EndOfYear(this DateTime date)`

Returns a `DateTime` set to the last moment of December 31 of the year that contains `date`.

### IsPast
`public static bool IsPast(this DateTime date)`

Returns `true` if `date` is strictly earlier than the current system time in UTC; otherwise `false`.

### IsFuture
`public static bool IsFuture(this DateTime date)`

Returns `true` if `date` is strictly later than the current system time in UTC; otherwise `false`.

### IsToday
`public static bool IsToday(this DateTime date)`

Returns `true` if `date` falls on the current calendar date (based on local time); otherwise `false`.

### GetAge
`public static int GetAge(this DateTime birthDate, DateTime? referenceDate = null)`

Calculates the number of full years between `birthDate` and the specified `referenceDate`. If `referenceDate` is `null`, the current date is used. Throws `ArgumentOutOfRangeException` when `birthDate` is later than the reference date.

### DaysBetween
`public static int DaysBetween(this DateTime start, DateTime end)`

Returns the absolute number of whole calendar days between `start` and `end`. The result is always zero or positive.

### GetTimeElapsed
`public static string GetTimeElapsed(this DateTime pastDate, DateTime? referenceDate = null)`

Produces a human-readable string describing the approximate time that has passed since `pastDate` (e.g., "3 hours ago", "2 days ago"). If `referenceDate` is `null`, the current date and time is used. Throws `ArgumentOutOfRangeException` when `pastDate` is later than the reference date.

### RoundToNearest
`public static DateTime RoundToNearest(this DateTime date, TimeSpan interval)`

Rounds `date` to the nearest multiple of `interval`. When the value lies exactly halfway between two multiples, it rounds up. Throws `ArgumentException` if `interval` is zero or negative.

### ToIso8601
`public static string ToIso8601(this DateTime date)`

Converts `date` to its ISO 8601 string representation (e.g., `2025-03-15T14:30:00`). The output format is adjusted based on the `Kind` property to include offset or UTC designator when appropriate.

### ToDateString
`public static string ToDateString(this DateTime date, string format = "yyyy-MM-dd")`

Returns the date portion of `date` as a string using the specified format. The default format is `yyyy-MM-dd`.

### ToDateTimeString
`public static string ToDateTimeString(this DateTime date, string format = "yyyy-MM-dd HH:mm:ss")`

Returns the date and time portion of `date` as a string using the specified format. The default format is `yyyy-MM-dd HH:mm:ss`.

## Usage

```csharp
// Calculate age and check if a given date is in the past
DateTime birthDate = new DateTime(1990, 5, 20);
int age = birthDate.GetAge(); // Uses current date as reference
bool isPast = birthDate.IsPast(); // Always true for historical dates

Console.WriteLine($"Age: {age}, IsPast: {isPast}");
```

```csharp
// Generate report boundaries and format output
DateTime now = DateTime.UtcNow;
DateTime monthStart = now.StartOfMonth();
DateTime monthEnd = now.EndOfMonth();

string range = $"{monthStart.ToIso8601()} to {monthEnd.ToIso8601()}";
string elapsed = monthStart.GetTimeElapsed();

Console.WriteLine($"Report range: {range}");
Console.WriteLine($"Month started: {elapsed}");
```

## Notes

- All extension methods that return a new `DateTime` preserve the `Kind` property of the input unless the documentation explicitly states otherwise. When comparing values of different `Kind`, results may be affected by automatic UTC-to-local conversions.
- `IsPast`, `IsFuture`, and `IsToday` rely on the system clock. They are not thread-blocking but are inherently non-deterministic; repeated calls may yield different results as time advances.
- `GetAge` and `GetTimeElapsed` throw `ArgumentOutOfRangeException` when the provided date is in the future relative to the reference date. Callers should validate input order when working with user-supplied dates.
- `RoundToNearest` throws `ArgumentException` for non-positive intervals. Rounding to intervals smaller than one tick (100 nanoseconds) is not supported and will produce an exception.
- The string-formatting methods (`ToIso8601`, `ToDateString`, `ToDateTimeString`) are culture-insensitive and use the invariant culture. They are safe for serialization and machine-to-machine exchange.
- All members are static and do not mutate shared state; they are safe to call concurrently from multiple threads without external synchronization.
