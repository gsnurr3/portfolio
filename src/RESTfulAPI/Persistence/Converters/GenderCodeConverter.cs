using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RESTfulAPI.Domain.Enums;

namespace RESTfulAPI.Persistence.Converters
{
    public sealed class GenderCodeConverter : ValueConverter<Gender, string>
    {
        // model -> db ('M','F','O')
        private static readonly Expression<Func<Gender, string>> ToDb =
            g => (g == Gender.Male) ? "M"
               : (g == Gender.Female) ? "F"
               : "O";

        // db -> model (case-insensitive)
        private static readonly Expression<Func<string, Gender>> FromDb =
            s => (s == "M" || s == "m") ? Gender.Male
               : (s == "F" || s == "f") ? Gender.Female
               : Gender.Other;

        public GenderCodeConverter() : base(ToDb, FromDb) { }
    }
}