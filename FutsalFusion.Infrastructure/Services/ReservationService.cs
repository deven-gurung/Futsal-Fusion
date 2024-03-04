using FutsalFusion.Domain.Entities.Identity;

namespace FutsalFusion.Infrastructure.Services;

public class ReservationService
{
    public List<FreeTermDto> GetFreeTerms(SportObject sportObject)
    {
        var workingHours = sportObject.WorkingHours.ToList();

        if (!workingHours.Any())
        {
            throw new Exception("Sport object doesn't have working hours");
        }

        var maxTimeWorkingHour = sportObject.WorkingHours.Min(wh => wh.CloseTime);

        if (maxTimeWorkingHour.Hours != 0)
        {
            maxTimeWorkingHour = sportObject.WorkingHours.Max(wh => wh.CloseTime);
        }

        var prices = sportObject.Prices.ToList();

        if (!prices.Any())
        {
            throw new Exception("Sport object doesn't have prices");
        }

        const int futureDays = 7;
        var reservations = sportObject.Reservations
            .Where(r => r.Date >= DateTime.Today && r.Date <= DateTime.Today.AddDays(futureDays))
            .ToList();

        var freeTerms = new List<FreeTermDto>();

        for (var i = 0; i <= futureDays; i++)
        {
            var date = DateTime.Today.AddDays(i);
            var day = (int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;
            var wh = workingHours.Find(w => w.Day == day);
            var openTimeHours = wh.OpenTime.Hours;
            var closeTimeHours = wh.CloseTime.Hours;

            if (closeTimeHours == 0)
            {
                closeTimeHours = 24;
            }

            for (var j = openTimeHours; j < closeTimeHours; j++)
            {
                var startTime = new TimeSpan(j, 0, 0);
                var endTime = startTime.Add(TimeSpan.FromHours(1));

                if (startTime.Hours == 23)
                {
                    endTime = new TimeSpan(0, 0, 0);
                }

                var price = sportObject.Prices.SingleOrDefault(p => startTime >= p.TimeFrom && startTime < p.TimeTo && p.TimeTo != maxTimeWorkingHour) ??
                            sportObject.Prices.SingleOrDefault(p => p.TimeTo == maxTimeWorkingHour);

                var freeTerm = new FreeTermDto
                {
                    Date = date,
                    StartTime = startTime,
                    Price = price.PricePerHour
                };

                freeTerms.Add(freeTerm);
            }
        }

        var output = new List<FreeTermDto>();

        foreach (var ft in freeTerms)
        {
            output.Add(ft);

            foreach (var res in reservations.Where(res => ft.Date == res.Date && ft.StartTime == res.StartTime))
            {
                output.Remove(ft);
            }
        }

        return output;
    }

    public List<TermByDateDto> GetAllTerms(SportObject sportObject)
    {
        var workingHours = sportObject.WorkingHours.ToList();

        if (workingHours.Count == 0)
        {
            throw new Exception("Sport object doesn't have working hours");
        }

        var maxTimeWorkingHour = sportObject.WorkingHours.Min(wh => wh.CloseTime);

        if (maxTimeWorkingHour.Hours != 0)
        {
            maxTimeWorkingHour = sportObject.WorkingHours.Max(wh => wh.CloseTime);
        }

        var prices = sportObject.Prices.ToList();

        if (prices.Count == 0)
        {
            throw new Exception("Sport object doesn't have prices");
        }

        const int futureDays = 7;
        var reservations = sportObject.Reservations
            .Where(r => r.Date >= DateTime.Today && r.Date <= DateTime.Today.AddDays(futureDays))
            .ToList();

        var termsByDate = new List<TermByDateDto>();

        for (var i = 0; i <= futureDays; i++)
        {
            var date = DateTime.Today.AddDays(i);
            var day = (int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;
            var wh = workingHours.Find(w => w.Day == day);
            var openTimeHours = wh.OpenTime.Hours;
            var closeTimeHours = wh.CloseTime.Hours;

            if (closeTimeHours == 0)
            {
                closeTimeHours = 24;
            }

            var termByDate = new TermByDateDto
            {
                Date = date
            };

            for (var j = openTimeHours; j < closeTimeHours; j++)
            {
                var startTime = new TimeSpan(j, 0, 0);
                var endTime = startTime.Add(TimeSpan.FromHours(1));

                if (startTime.Hours == 23)
                {
                    endTime = new TimeSpan(0, 0, 0);
                }

                var price = sportObject.Prices.SingleOrDefault(p => startTime >= p.TimeFrom && startTime < p.TimeTo && p.TimeTo != maxTimeWorkingHour) ??
                            sportObject.Prices.SingleOrDefault(p => p.TimeTo == maxTimeWorkingHour);

                var term = new TermDto
                {
                    StartTime = startTime,
                    Price = price!.PricePerHour,
                    Status = "Free",
                    IsExpired = false
                };

                termByDate.Terms.Add(term);

            }

            termsByDate.Add(termByDate);
        }


        foreach (var res in reservations)
        {
            foreach (var termByDate in termsByDate)
            {
                foreach(var term in termByDate.Terms)
                {
                    if (termByDate.Date == res.Date && term.StartTime == res.StartTime)
                    {
                        term.Status = res.EndTime.ToString();
                    }

                    if (termByDate.Date == DateTime.Today && term.StartTime < DateTime.Now.TimeOfDay)
                    {
                        term.Status = "Accepted";
                    }
                }
            }
        }

        return termsByDate;
    }
}

public class TermByDateDto
{
    public DateTime Date { get; set; }
    
    public IList<TermDto> Terms { get; set; } = new List<TermDto>();
}

public class TermDto
{
    public TimeSpan StartTime { get; set; }
    
    public int Price { get; set; }
    
    public string Status { get; set; }
    
    public bool IsExpired { get; set; }
}

public class SportObject
{
    public int Id { get; set; }
    
    public string Email { get; set; }
    
    public string Name { get; set; }
    
    public string Address { get; set; }
    
    public string Phone { get; set; }
    
    public string Description { get; set; }
    
    public bool IsPayed { get; set; }
    
    public bool IsPremium { get; set; }
    
    public int SportId { get; set; }
    
    public int CityId { get; set; }

    public virtual ICollection<Price> Prices { get; set; }
    
    public virtual ICollection<WorkingHour> WorkingHours { get; set; }
    
    public virtual ICollection<Review> Reviews { get; set; }
    
    public virtual ICollection<Reservation> Reservations { get; set; }
    
    public virtual ICollection<Favourite> Favourites { get; set; }
    
    public virtual ICollection<Image> Images { get; set; }
}

public class Image
{
    public string Id { get; set; }
    
    public string Url { get; set; }
    
    public bool IsMain { get; set; }
    
    public int SportObjectId { get; set; }

    public virtual SportObject SportObject { get; set; }
}

public class Favourite
{
    public int Id { get; set; }
    
    public string UserId { get; set; }
    
    public int SportObjectId { get; set; }

    public virtual User User { get; set; }
    
    public virtual SportObject SportObject { get; set; }
}

public class Reservation
{
    public int Id { get; set; }
    
    public int SportObjectId { get; set; }
    
    public string UserId { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public DateTime Date { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int Price { get; set; }
    
    public int StatusId { get; set; }
}

public class Review
{
    public string UserId { get; set; }
    
    public int SportObjectId { get; set; }
    
    public int Rating { get; set; }
    
    public string Comment { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }
    
    public virtual SportObject SportObject { get; set; }
}

public class WorkingHour
{
    public int Id { get; set; }
    
    public int Day { get; set; }
    
    public TimeSpan OpenTime { get; set; }
    
    public TimeSpan CloseTime { get; set; }
    
    public int SportObjectId { get; set; }

    public virtual SportObject SportObject { get; set; }
}

public class Price
{
    public int Id { get; set; }
    
    public int PricePerHour { get; set; }
    
    public TimeSpan TimeFrom { get; set; }
    
    public TimeSpan TimeTo { get; set; }
    
    public int SportObjectId { get; set; }

    public virtual SportObject SportObject { get; set; }
}

public class FreeTermDto
{
    public DateTime Date { get; set; }
    
    public TimeSpan StartTime { get; set; }

    public int Price { get; set; }
}