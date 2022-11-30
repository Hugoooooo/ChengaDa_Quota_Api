using AutoMapper;
using ChengDaApi.DBRepositories.DBSchema;
using Domain.Models.Quote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PunchRecord, PunchDetail>()
                .ForMember(dis => dis.on_work, from => from.MapFrom(o => o.上班時間))
                .ForMember(dis => dis.off_work, from => from.MapFrom(o => o.下班時間));
        }
    }
}
