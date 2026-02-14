using System.Net.Http.Json;

namespace feriado.Services;

public class HolidayService(HttpClient httpClient)
{
    public async Task<List<DateOnly>> GetHolidaysAsync()
    {
        var year = DateTime.Now.Year;
        var url = $"https://date.nager.at/api/v3/publicholidays/{year}/BR";

        try
        {
            var holidays = await httpClient.GetFromJsonAsync<List<HolidayDto>>(url);

            if (holidays is null) return [];
            
            return holidays
                .Where(h => h.Global || (h.Counties is not null && h.Counties.Contains("BR-SP")))
                .Select(h => DateOnly.Parse(h.Date))
                .Distinct()
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private class HolidayDto
    {
        public string Date { get; set; } = string.Empty;
        public bool Global { get; set; }
        public string[]? Counties { get; set; }
    }
}