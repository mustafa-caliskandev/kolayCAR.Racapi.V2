using Microsoft.Extensions.Configuration;
using System;

namespace kolayCAR.Broker.AWS.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static string GetSectionValueString(
            this IConfiguration configuration,
            string sectionName, string subSectionName = "")
        {
            if (string.IsNullOrEmpty(subSectionName))
            {
                return configuration.GetSection(sectionName).Value ?? "";
            }
            else
            {
                return configuration.GetSection(sectionName).GetSection(subSectionName).Value ?? "";
            }
        }

        public static int GetSectionValueInt(
            this IConfiguration configuration,
            string sectionName, string subSectionName = "")
        {
            if (string.IsNullOrEmpty(subSectionName))
            {
                return Convert.ToInt32(configuration.GetSection(sectionName).Value ?? "0");
            }
            else
            {
                return Convert.ToInt32(configuration.GetSection(sectionName).GetSection(subSectionName).Value ?? "0");
            }
        }

        public static bool GetSectionValueBool(
            this IConfiguration configuration,
            string sectionName, string subSectionName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(subSectionName))
                {
                    return Convert.ToBoolean(configuration.GetSection(sectionName).Value ?? "false");
                }
                else
                {
                    return Convert.ToBoolean(configuration.GetSection(sectionName).GetSection(subSectionName).Value ?? "false");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
