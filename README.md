// existing content ...

## MetricCorrelationDto

The `MetricCorrelationDto` class represents a correlation between two metrics, providing detailed information about the relationship between them, including the correlation coefficient, strength, direction, and sample count.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new MetricCorrelationDto instance
var correlation = new MetricCorrelationDto
{
    Pair = new CorrelationPair("HeartRate", "Steps"),
    Coefficient = 0.75,
    Strength = CorrelationStrength.Strong,
    Direction = CorrelationDirection.Direct,
    SampleCount = 100,
    Interpretation = "Strong direct relationship between HeartRate and Steps",
    AnalysisPeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
    AnalysisPeriodEnd = DateOnly.FromDateTime(DateTime.Now)
};

// Check if the correlation is significant
bool isSignificant = correlation.IsSignificant();
Console.WriteLine($"Is significant: {isSignificant}");

// Get the title and description of the correlation
string title = correlation.Title;
string description = correlation.Description;
Console.WriteLine($"Title: {title}");
Console.WriteLine($"Description: {description}");
```

