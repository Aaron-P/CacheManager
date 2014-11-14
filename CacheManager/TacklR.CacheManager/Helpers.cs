﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace TacklR.CacheManager
{
    internal static class Helpers
    {
        private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static float? GetAvailableMemory()
        {
            try
            {
                using (var pc = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    return pc.NextValue();
                }
            }
            catch
            {
                return default(float?);
            }
        }

        private static NameValueCollection s_SecurityHeaders { get; set; }
        internal static NameValueCollection SecurityHeaders
        {
            get
            {
                if (s_SecurityHeaders == null)
                {
                    //What if the header is already set?
                    s_SecurityHeaders = new NameValueCollection {
                        { "X-Frame-Options", "SameOrigin" },
                        { "X-Content-Type-Options", "nosniff" },
                        { "X-XSS-Protection", "1; mode=block" }
                    };
                }
                return s_SecurityHeaders;
            }
        }

        //Can we modify the 'this' value?
        internal static void Override(this NameValueCollection collection, NameValueCollection overrides)
        {
            overrides.AllKeys.ToList().ForEach(k => collection.Remove(k));
            collection.Add(overrides);
        }

        internal static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        internal static long? ToUnixMilliseconds(this DateTime? time)//long?
        {
            if (!time.HasValue)
                return default(long?);
            return time.Value.ToUnixMilliseconds();
        }

        internal static long ToUnixMilliseconds(this DateTime time)
        {
            var difference = time - UnixEpochUtc;
            var timestamp = difference.TotalMilliseconds;
            return (long)timestamp;
        }

        internal static long? ToMilliseconds(this TimeSpan? timespan)
        {
            if (!timespan.HasValue)
                return default(long?);
            return (long)timespan.Value.TotalMilliseconds;
        }

        internal static bool TryChangeType(object value, Type type, out object converted)
        {
            try
            {
                converted = Convert.ChangeType(value, type);
                return true;
            }
            catch (Exception)
            {
                //log?
                converted = default(object);
                return false;
            }
        }
    }

    internal static partial class AntiForgeryHelpers
    {
        //Return something better than tuple?
        private static Tuple<string, string> GetTokens(string oldCookieToken = null)
        {
            string cookieToken, formToken;
            AntiForgery.GetTokens(oldCookieToken, out cookieToken, out formToken);
            return Tuple.Create(cookieToken, formToken);
        }

        //Return something better than tuple?
        internal static Tuple<HttpCookie, string> GetVerificationTokenContent(HttpCookieCollection cookies = null, string valueId = "VerificationToken", string cookieName = "X-CSRF-Token")
        {
            var oldCookieToken = default(string);
            if (cookies != null)
            {
                if (cookies.AllKeys.Contains(cookieName))
                {
                    var oldCookie = cookies.Get(cookieName);//why does this create a cookie if it doesn't exist...
                    if (oldCookie != null)
                        oldCookieToken = oldCookie.Value;
                }
            }

            var tokens = GetTokens(oldCookieToken);
            var cookie = tokens.Item1 != null && tokens.Item1 != oldCookieToken ? new HttpCookie(cookieName, tokens.Item1) { HttpOnly = true, Secure = false } : default(HttpCookie);//TODO: Set cookie secure if we are always on secure, need to get that from a setting or something.
            var content = String.Format("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>&#3232;_&#3232;</title></head><input type=\"hidden\" id=\"{0}\" value=\"{1}\"></html>", valueId, tokens.Item2);
            return Tuple.Create(cookie, content);
        }
    }
}
