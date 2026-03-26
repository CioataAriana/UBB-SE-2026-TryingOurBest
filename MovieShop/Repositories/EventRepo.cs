using Microsoft.Data.SqlClient;
using MovieShop.Models;
using System;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public sealed class EventRepo
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        public List<MovieEvent> GetEventsForMovie(int movieId)
        {
            var list = new List<MovieEvent>();
            const string query = @"
SELECT ID, MovieID, Title, Description, Date, Location, TicketPrice, PosterUrl
FROM Events
WHERE MovieID = @mid
ORDER BY Date ASC, ID ASC";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, _db.Connection);
                cmd.Parameters.AddWithValue("@mid", movieId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MovieEvent
                    {
                        ID = reader.GetInt32(0),
                        MovieID = reader.GetInt32(1),
                        Title = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Date = reader.GetDateTime(4),
                        Location = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        TicketPrice = reader.GetDecimal(6),
                        PosterUrl = reader.IsDBNull(7) ? "" : reader.GetString(7)
                    });
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }
    }
}

