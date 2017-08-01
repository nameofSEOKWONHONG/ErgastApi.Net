﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ErgastApi.Client.Attributes;
using ErgastApi.Requests;

namespace ErgastApi.Client
{
    public class UrlBuilder : IUrlBuilder
    {
        public string Build(IErgastRequest request)
        {
            var segments = GetSegments(request);

            var url = "";
            foreach (var segment in segments)
            {
                if (segment.Name != null)
                    url += $"/{segment.Name}";

                if (segment.Value != null)
                    url += $"/{segment.Value}";
            }

            url += ".json";

            if (request.Limit != null)
                url += "?limit=" + request.Limit;

            if (request.Offset != null)
            {
                url += request.Limit == null ? "?" : "&";
                url += "offset=" + request.Offset;
            }

            return url;
        }

        // TODO: Extend to check for UrlSegmentDependencyAttribute and that the dependent property value is not null
        private static IList<UrlSegmentInfo> GetSegments(IErgastRequest request)
        {
            var segments = new List<UrlSegmentInfo>();
            var properties = request.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in properties)
            {
                var urlSegment = prop.GetCustomAttributes<UrlSegmentAttribute>(true).FirstOrDefault();
                var urlTerminator = prop.GetCustomAttributes<UrlTerminatorAttribute>(true).FirstOrDefault();

                if (urlSegment == null)
                    continue;

                // TODO: Expand UrlSegmentInfo with more info like PropertyInfo etc.

                var segment = new UrlSegmentInfo
                {
                    Name = urlSegment.MethodName,
                    Order = NormalizeOrder(urlSegment.Order),
                    Value = GetSegmentValue(prop, request),
                    IsTerminator = urlTerminator != null
                };

                if (segment.Value == null && !segment.IsTerminator)
                    continue;

                segments.Add(segment);
            }

            segments.Sort();

            return segments;
        }

        private static string GetSegmentValue(PropertyInfo property, IErgastRequest request)
        {
            var value = property.GetValue(request);
            if (value?.GetType().IsEnum == true)
                value = (int) value;

            return value?.ToString();
        }

        /// <summary>
        /// Converts 0 to null.
        /// </summary>
        private static int? NormalizeOrder(int order)
        {
            return order == 0 ? null : (int?) order;
        }
    }
}