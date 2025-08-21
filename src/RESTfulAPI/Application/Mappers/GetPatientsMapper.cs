using AutoMapper;
using RESTfulAPI.Application.DTOs;
using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Application.Mappers
{
    public sealed class GetPatientsMapper : Profile
    {
        public GetPatientsMapper()
        {
            CreateMap<Patient, GetPatientsDTO>();
        }
    }
}