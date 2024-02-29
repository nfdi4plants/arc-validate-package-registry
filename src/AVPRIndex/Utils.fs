namespace AVPRIndex

open System

module Utils = 

    let truncateDateTime (date: System.DateTimeOffset) =
        DateTimeOffset.ParseExact(
            date.ToString("yyyy-MM-dd HH:mm:ss zzzz"), 
            "yyyy-MM-dd HH:mm:ss zzzz", 
            System.Globalization.CultureInfo.InvariantCulture
        )