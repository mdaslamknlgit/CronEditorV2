using System.Text.RegularExpressions;

namespace CronEditorV2.Services;

public class CronScheduleItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CronBuilderService
{
    private readonly List<CronScheduleItem> _schedules = new()
    {
        new CronScheduleItem
        {
            Name = "Every Minute",
            Expression = "* * * * *",
            Description = "Runs every minute",
            IsActive = true
        },
        new CronScheduleItem
        {
            Name = "Daily Midnight",
            Expression = "0 0 * * *",
            Description = "Runs at midnight every day",
            IsActive = true
        },
        new CronScheduleItem
        {
            Name = "Weekly Sunday",
            Expression = "0 0 * * 0",
            Description = "Runs at midnight every Sunday",
            IsActive = false
        }
    };

    public List<CronScheduleItem> GetSchedules() => _schedules.ToList();

    public CronScheduleItem? GetSchedule(Guid id) =>
        _schedules.FirstOrDefault(s => s.Id == id);

    public void AddSchedule(CronScheduleItem item)
    {
        item.Id = Guid.NewGuid();
        item.CreatedAt = DateTime.UtcNow;
        _schedules.Add(item);
    }

    public bool UpdateSchedule(CronScheduleItem item)
    {
        var existing = _schedules.FirstOrDefault(s => s.Id == item.Id);
        if (existing is null) return false;

        existing.Name = item.Name;
        existing.Expression = item.Expression;
        existing.Description = item.Description;
        existing.IsActive = item.IsActive;
        return true;
    }

    public bool DeleteSchedule(Guid id)
    {
        var item = _schedules.FirstOrDefault(s => s.Id == id);
        if (item is null) return false;
        _schedules.Remove(item);
        return true;
    }

    public (bool IsValid, string ErrorMessage) ValidateCronExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return (false, "Expression cannot be empty.");

        var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 5 || parts.Length > 6)
            return (false, $"Expression must have 5 or 6 fields (got {parts.Length}). Format: minute hour day month dayOfWeek [year]");

        var (minuteValid, minuteErr) = ValidateField(parts[0], 0, 59, "Minute");
        if (!minuteValid) return (false, minuteErr);

        var (hourValid, hourErr) = ValidateField(parts[1], 0, 23, "Hour");
        if (!hourValid) return (false, hourErr);

        var (dayValid, dayErr) = ValidateField(parts[2], 1, 31, "Day");
        if (!dayValid) return (false, dayErr);

        var (monthValid, monthErr) = ValidateMonthField(parts[3]);
        if (!monthValid) return (false, monthErr);

        var (dowValid, dowErr) = ValidateDayOfWeekField(parts[4]);
        if (!dowValid) return (false, dowErr);

        if (parts.Length == 6)
        {
            var (yearValid, yearErr) = ValidateField(parts[5], 1970, 2099, "Year");
            if (!yearValid) return (false, yearErr);
        }

        return (true, string.Empty);
    }

    private static (bool, string) ValidateField(string field, int min, int max, string name)
    {
        if (field == "*") return (true, string.Empty);
        if (field.Contains('/'))
        {
            var stepParts = field.Split('/');
            if (stepParts.Length != 2) return (false, $"{name}: invalid step expression '{field}'.");
            if (stepParts[0] != "*" && !int.TryParse(stepParts[0], out _))
                return (false, $"{name}: invalid step base '{stepParts[0]}'.");
            if (!int.TryParse(stepParts[1], out var step) || step < 1)
                return (false, $"{name}: step value must be a positive integer.");
            return (true, string.Empty);
        }
        if (field.Contains(','))
        {
            foreach (var part in field.Split(','))
            {
                var (v, e) = ValidateField(part.Trim(), min, max, name);
                if (!v) return (false, e);
            }
            return (true, string.Empty);
        }
        if (field.Contains('-'))
        {
            var rangeParts = field.Split('-');
            if (rangeParts.Length != 2) return (false, $"{name}: invalid range '{field}'.");
            if (!int.TryParse(rangeParts[0], out var rangeMin) || rangeMin < min || rangeMin > max)
                return (false, $"{name}: range start must be between {min} and {max}.");
            if (!int.TryParse(rangeParts[1], out var rangeMax) || rangeMax < min || rangeMax > max)
                return (false, $"{name}: range end must be between {min} and {max}.");
            if (rangeMin > rangeMax)
                return (false, $"{name}: range start must be less than or equal to range end.");
            return (true, string.Empty);
        }
        if (!int.TryParse(field, out var value))
            return (false, $"{name}: '{field}' is not a valid number.");
        if (value < min || value > max)
            return (false, $"{name}: value {value} is out of range [{min}-{max}].");
        return (true, string.Empty);
    }

    private static (bool, string) ValidateMonthField(string field)
    {
        var monthNames = new[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN",
                                  "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
        var normalized = field.ToUpperInvariant();
        for (int i = 0; i < monthNames.Length; i++)
            normalized = normalized.Replace(monthNames[i], (i + 1).ToString());
        return ValidateField(normalized, 1, 12, "Month");
    }

    private static (bool, string) ValidateDayOfWeekField(string field)
    {
        var dowNames = new[] { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
        var normalized = field.ToUpperInvariant();
        for (int i = 0; i < dowNames.Length; i++)
            normalized = normalized.Replace(dowNames[i], i.ToString());
        // Allow 0-7 (both 0 and 7 represent Sunday, as per POSIX cron standard)
        return ValidateField(normalized, 0, 7, "Day of Week");
    }

    public string BuildCronExpression(string minute, string hour, string day, string month, string dayOfWeek, string year = "")
    {
        minute = string.IsNullOrWhiteSpace(minute) ? "*" : minute.Trim();
        hour = string.IsNullOrWhiteSpace(hour) ? "*" : hour.Trim();
        day = string.IsNullOrWhiteSpace(day) ? "*" : day.Trim();
        month = string.IsNullOrWhiteSpace(month) ? "*" : month.Trim();
        dayOfWeek = string.IsNullOrWhiteSpace(dayOfWeek) ? "*" : dayOfWeek.Trim();

        var expr = $"{minute} {hour} {day} {month} {dayOfWeek}";
        if (!string.IsNullOrWhiteSpace(year))
            expr += $" {year.Trim()}";
        return expr;
    }

    public string GetDescription(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression)) return "No expression provided.";

        var (isValid, error) = ValidateCronExpression(expression);
        if (!isValid) return $"Invalid expression: {error}";

        var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var minute = parts[0];
        var hour = parts[1];
        var day = parts[2];
        var month = parts[3];
        var dow = parts[4];
        var year = parts.Length > 5 ? parts[5] : "*";

        var description = "Runs ";

        // Time description
        if (minute == "*" && hour == "*")
            description += "every minute";
        else if (minute == "*")
            description += $"every minute of hour {hour}";
        else if (hour == "*")
            description += $"at minute {minute} of every hour";
        else if (minute.Contains("*/"))
            description += $"every {minute.Split('/')[1]} minutes";
        else
            description += $"at {hour.PadLeft(2, '0')}:{minute.PadLeft(2, '0')}";

        // Day description
        if (dow != "*")
            description += $" on {ExpandDayOfWeek(dow)}";
        else if (day != "*")
            description += $" on day {day} of the month";
        else
            description += " every day";

        // Month description
        if (month != "*")
            description += $" in {ExpandMonth(month)}";

        // Year description
        if (year != "*" && !string.IsNullOrWhiteSpace(year))
            description += $" in year {year}";

        return description;
    }

    private static string ExpandDayOfWeek(string dow)
    {
        var names = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        if (dow == "*") return "every day of the week";
        if (dow.Contains("*/"))
        {
            var step = dow.Split('/')[1];
            return $"every {step} days of the week";
        }
        if (dow.Contains(','))
        {
            var parts = dow.Split(',').Select(p => ExpandDayOfWeekSingle(p.Trim(), names));
            return string.Join(", ", parts);
        }
        if (dow.Contains('-'))
        {
            var parts = dow.Split('-');
            if (parts.Length == 2)
                return $"{ExpandDayOfWeekSingle(parts[0], names)} through {ExpandDayOfWeekSingle(parts[1], names)}";
        }
        return ExpandDayOfWeekSingle(dow, names);
    }

    private static string ExpandDayOfWeekSingle(string dow, string[] names)
    {
        if (int.TryParse(dow, out var idx))
        {
            if (idx == 7) return "Sunday"; // 7 is also Sunday in POSIX cron
            if (idx >= 0 && idx <= 6) return names[idx];
        }
        return dow;
    }

    private static string ExpandMonth(string month)
    {
        var names = new[] { "", "January", "February", "March", "April", "May", "June",
                             "July", "August", "September", "October", "November", "December" };
        if (month == "*") return "every month";
        if (month.Contains("*/"))
        {
            var step = month.Split('/')[1];
            return $"every {step} months";
        }
        if (month.Contains(','))
        {
            var parts = month.Split(',').Select(p => ExpandMonthSingle(p.Trim(), names));
            return string.Join(", ", parts);
        }
        if (month.Contains('-'))
        {
            var parts = month.Split('-');
            if (parts.Length == 2)
                return $"{ExpandMonthSingle(parts[0], names)} through {ExpandMonthSingle(parts[1], names)}";
        }
        return ExpandMonthSingle(month, names);
    }

    private static string ExpandMonthSingle(string month, string[] names)
    {
        if (int.TryParse(month, out var idx) && idx >= 1 && idx <= 12)
            return names[idx];
        return month;
    }

    public List<(string Name, string Expression)> GetPresetSchedules() => new()
    {
        ("Every Minute",             "* * * * *"),
        ("Every 5 Minutes",          "*/5 * * * *"),
        ("Every 15 Minutes",         "*/15 * * * *"),
        ("Every 30 Minutes",         "*/30 * * * *"),
        ("Every Hour",               "0 * * * *"),
        ("Every 6 Hours",            "0 */6 * * *"),
        ("Every 12 Hours",           "0 */12 * * *"),
        ("Daily at Midnight",        "0 0 * * *"),
        ("Daily at Noon",            "0 12 * * *"),
        ("Daily at 6 AM",            "0 6 * * *"),
        ("Weekly on Sunday",         "0 0 * * 0"),
        ("Weekly on Monday",         "0 0 * * 1"),
        ("Monthly (1st at Midnight)","0 0 1 * *"),
        ("Quarterly (1st Jan/Apr/Jul/Oct)", "0 0 1 1,4,7,10 *"),
        ("Yearly (Jan 1st at Midnight)",    "0 0 1 1 *"),
        ("Weekdays at 9 AM",         "0 9 * * 1-5"),
        ("Weekends at Noon",         "0 12 * * 6,0"),
    };
}
