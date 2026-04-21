using AutoMapper;
using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Agency, RestrictedAgency>();
            CreateMap<Reservation, RestrictedReservation>();
            CreateMap<IEnumerable<Reservation>, IEnumerable<RestrictedReservation>>();
            CreateMap<Vehicle, RestrictedVehicle>();
            CreateMap<Extra, RestrictedExtra>();
            CreateMap<ReservationExtra, RestrictedReservationExtra>();
            CreateMap<ReservationExtra, ReservationExtra>();
            CreateMap<Extra, ExtraListItem>();
            CreateMap<Addition, Addition>();
        }
    }
}
