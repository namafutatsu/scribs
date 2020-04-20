using System;
using AutoMapper;
using Scribs.Core.Entities;
using Scribs.Core.Models;

namespace Scribs.Core.Services {

    public static class MapperUtils {

        public static IMapper GetMapper() {
            var mapperConfig = new MapperConfiguration(configuration => {
                configuration.CreateMap<User, UserRegistrationModel>().ReverseMap();
                configuration.CreateMap<Document, DocumentModel>().ReverseMap();
                configuration.CreateMap<Text, TextModel>().ReverseMap();
                configuration.IgnoreUnmapped();
            });
            mapperConfig.AssertConfigurationIsValid();
            return mapperConfig.CreateMapper();
        }

        private static void IgnoreUnmappedProperties(TypeMap map, IMappingExpression expr) {
            foreach (string propName in map.GetUnmappedPropertyNames()) {
                var srcPropInfo = map.SourceType.GetProperty(propName);

                var destPropInfo = map.DestinationType.GetProperty(propName);

                if (destPropInfo != null)
                    expr.ForMember(propName, opt => opt.Ignore());
            }
        }

        public static void IgnoreUnmapped(this IProfileExpression profile) {
            profile.ForAllMaps(IgnoreUnmappedProperties);
        }

        public static void IgnoreUnmapped(this IProfileExpression profile, Func<TypeMap, bool> filter) {
            profile.ForAllMaps((map, expr) => {
                if (filter(map)) {
                    IgnoreUnmappedProperties(map, expr);
                }
            });
        }

        public static void IgnoreUnmapped(this IProfileExpression profile, Type src, Type dest) {
            profile.IgnoreUnmapped((TypeMap map) => map.SourceType == src && map.DestinationType == dest);
        }

        public static void IgnoreUnmapped<TSrc, TDest>(this IProfileExpression profile) {
            profile.IgnoreUnmapped(typeof(TSrc), typeof(TDest));
        }
    }
}
