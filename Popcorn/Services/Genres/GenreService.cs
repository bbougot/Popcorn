using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Popcorn.Models.Genres;

namespace Popcorn.Services.Genres
{
    public class GenreService : IGenreService
    {
        /// <summary>
        /// Get all genres
        /// </summary>
        /// <param name="language">Genre language</param>
        /// <param name="ct">Used to cancel loading genres</param>
        /// <returns>Genres</returns>
        public async Task<List<GenreJson>> GetGenresAsync(string language, CancellationToken ct)
        {
            var response = new GenreResponse
            {
                Genres = new List<GenreJson>
                {
                    new GenreJson
                    {
                        EnglishName = "Action",
                        Name = language == "es" ? "Acción" : "Action"
                    },
                    new GenreJson
                    {
                        EnglishName = "Adventure",
                        Name = language == "fr" ? "Aventure" : (language == "es" ? "Aventura" : "Adventure")
                    },
                    new GenreJson
                    {
                        EnglishName = "Animation",
                        Name = language == "es" ? "Animación" : "Animation"
                    },
                    new GenreJson
                    {
                        EnglishName = "Comedy",
                        Name = language == "fr" ? "Comédie" : (language == "es" ? "Comedia" : "Comedy")
                    },
                    new GenreJson
                    {
                        EnglishName = "Crime",
                        Name = language == "es" ? "Crimen" : "Crime"
                    },
                    new GenreJson
                    {
                        EnglishName = "Documentary",
                        Name = language == "fr" ? "Documentaire" : (language == "es" ? "Documental" : "Documentary")
                    },
                    new GenreJson
                    {
                        EnglishName = "Drama",
                        Name = language == "fr" ? "Drame" : "Drama"
                    },
                    new GenreJson
                    {
                        EnglishName = "Family",
                        Name = language == "fr" ? "Familial" : (language == "es" ? "Familiar" : "Family")
                    },
                    new GenreJson
                    {
                        EnglishName = "Fantasy",
                        Name = language == "fr" ? "Fantastique" : (language == "es" ? "Fantasía" : "Fantasy")
                    },
                    new GenreJson
                    {
                        EnglishName = "History",
                        Name = language == "fr" ? "Histoire" : (language == "es" ? "Historia" : "History")
                    },
                    new GenreJson
                    {
                        EnglishName = "Horror",
                        Name = language == "fr" ? "Horreur" : "Horror"
                    },
                    new GenreJson
                    {
                        EnglishName = "Music",
                        Name = language == "fr" ? "Musique" : (language == "es" ? "Musical" : "Music")
                    },
                    new GenreJson
                    {
                        EnglishName = "Mystery",
                        Name = language == "fr" ? "Mystère" : (language == "es" ? "Misterio" : "Mystery")
                    },
                    new GenreJson
                    {
                        EnglishName = "Romance",
                        Name = "Romance"
                    },
                    new GenreJson
                    {
                        EnglishName = "Science-Fiction",
                        Name = language == "es" ? "Ciencia-Ficción" : "Science-Fiction"
                    },
                    new GenreJson
                    {
                        EnglishName = "Thriller",
                        Name = language == "es" ? "Suspense" : "Thriller"
                    },
                    new GenreJson
                    {
                        EnglishName = "War",
                        Name = language == "fr" ? "Guerre" : (language == "es" ? "Bélica" : "War")
                    },
                    new GenreJson
                    {
                        EnglishName = "Western",
                        Name = language == "es" ? "Oeste" : "Western"
                    },
                }
            };

            return await Task.FromResult(response.Genres);
        }
    }
}