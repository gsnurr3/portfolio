using System.Globalization;
using AutoMapper;
using RESTfulAPI.Application.DTOs;
using RESTfulAPI.Application.Requests;
using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Application.Mappers
{
    public sealed class PostPatientMapper : Profile
    {
        public PostPatientMapper()
        {
            CreateMap<Patient, PostPatientDTO>();

            CreateMap<PostPatientRequest, Patient>()
            .ForMember(d => d.PatientId, o => o.Ignore()) // DB generates
            .ForMember(d => d.MedicalRecordNumber, o => o.MapFrom(s => s.MedicalRecordNumber.Trim().ToUpperInvariant()))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => ToTitle(s.FirstName)))
            .ForMember(d => d.LastName, o => o.MapFrom(s => ToTitle(s.LastName)))
            .ForMember(d => d.Address, o => o.MapFrom(s => NullIfEmpty(s.Address)))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => NormalizePhone(s.PhoneNumber)))
            .ForMember(d => d.Email, o => o.MapFrom(s => NormalizeEmail(s.Email)))
            .ForMember(d => d.InsurancePolicyNumber, o => o.MapFrom(s => NullIfEmpty(s.InsurancePolicyNumber)))
            .ForMember(d => d.CreatedAt, o => o.Ignore()) // DB defaults 
            .ForMember(d => d.UpdatedAt, o => o.Ignore()) // DB defaults
            .ForMember(d => d.InsuranceProvider, o => o.Ignore())
            .ForMember(d => d.Payments, o => o.Ignore());
        }

        private static string ToTitle(string s)
        {
            var t = s.Trim();
            if (t.Length == 0) return t;
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(t.ToLowerInvariant());
        }

        private static string? NormalizeEmail(string? email)
            => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

        private static string? NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 10)
                return $"({digits[..3]}){digits.Substring(3, 3)}-{digits.Substring(6)}";
            return phone.Trim();
        }

        private static string? NullIfEmpty(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}