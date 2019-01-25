﻿using Microsoft.Extensions.Configuration;

namespace TeamspeakAnalytics.hosting.Helper
{
  public static class ExtentionMethods
  {
    public static T GetSection<T>(this IConfiguration configuration)
    {
      var section = configuration.GetSection(typeof(T).Name);
      return section.Get<T>();
    }
  }
}