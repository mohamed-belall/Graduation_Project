﻿using AutoMapper;
using Graduation_Project.Core.Models;

namespace Graduation_Project.Api.Helpers
{
    public class PictureUrlResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, string>
        where TSource : BaseEntity
    {
        private readonly IConfiguration _configuration;

        public PictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(TSource source, TDestination destination, string destMember, ResolutionContext context)
        {
            var propertyNames = new[] { "PictureUrl", "MedicalImage" };
            var pictureUrl = GetFirstValidImageUrl(source, propertyNames);
            return pictureUrl;

        }

        private string GetFirstValidImageUrl(TSource source, string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = typeof(TSource).GetProperty(propertyName);
                var value = property?.GetValue(source) as string;

                if (!string.IsNullOrEmpty(value))
                    return value.StartsWith("/") ? $"{_configuration["ServerUrl"]}{value}" : $"{_configuration["ServerUrl"]}/{value}";
            }
            return null;
        }
    }
}