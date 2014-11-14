﻿using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using TacklR.CacheManager.Caches;
using TacklR.CacheManager.HttpHandlers;
using TacklR.CacheManager.Interfaces;
using TacklR.CacheManager.Models.Api;

namespace TacklR.CacheManager.Controllers
{
    //TODO: Attribute to control GET/POST?
    //Should every method refresh stats so they are as up-to-date as possible?
    internal class ApiController : DataHandler
    {
        private bool ValidateTokenHeader(string CookieName = "X-CSRF-Token", string HeaderName = "X-CSRF-Token")
        {
            var cookieToken = default(string);
            var cookies = HttpContext.Current.Request.Cookies;//can we get this from actionContext?
            if (cookies.AllKeys.Contains(CookieName))
            {
                var cookie = cookies.Get(CookieName);//why does this create a cookie if it doesn't exist...
                if (cookie != null)//blank check?
                    cookieToken = cookie.Value;
            }

            if (String.IsNullOrEmpty(cookieToken))
                return false;

            var headers = Request.Headers.Get(HeaderName);
            if (headers == null)
                return false;

            var values = headers.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var headerToken = values.FirstOrDefault();
            if (String.IsNullOrWhiteSpace(headerToken))
                return false;

            try
            {
                //Depreciated, replace with ValidateAntiForgeryToken and insert token into post body?
                AntiForgery.Validate(cookieToken, headerToken);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal IHttpHandler Cache()
        {
            var cache = new HttpCacheShim();
            var model = new CacheViewModel(cache) { Success = true };
            return base.Json(model);
        }

        //combine with delete? not sure how we would want to handle that
        //HttpPost
        internal IHttpHandler Clear()
        {
            if (!ValidateTokenHeader())
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Invalid verification token." }, HttpStatusCode.BadRequest);

            var cache = new HttpCacheShim();
            cache.Clear();
            return base.Json(new ClearViewModel { Success = true });
        }

        internal IHttpHandler Combined()
        {
            var cache = new HttpCacheShim() as ICache;
            var model = new CombinedViewModel(cache, HttpContext.Current) { Success = true };
            return base.Json(model);
        }

        //HttpPost
        internal IHttpHandler Delete(string key, bool prefix = false)
        {
            if (!ValidateTokenHeader())
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Invalid verification token." }, HttpStatusCode.BadRequest);

            if (String.IsNullOrEmpty(key))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Missing cache key or prefix." }, HttpStatusCode.BadRequest);

            var cache = new HttpCacheShim();
            cache.Clear(key, prefix);
            return base.Json(new DeleteViewModel(cache) { Success = true });
        }

        internal IHttpHandler Details(string key)
        {
            if (String.IsNullOrEmpty(key))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Missing cache key." }, HttpStatusCode.BadRequest);

            var cache = new HttpCacheShim() as ICache;
            var entry = cache.GetEntry(key);
            if (entry == default(CacheEntry))//better check?
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Invalid cache key." }, HttpStatusCode.BadRequest);

            var model = new DetailsViewModel(entry) { Success = true };
            return base.Json(model);
        }

        //HttpPost
        internal IHttpHandler Page(string url)
        {
            if (!ValidateTokenHeader())
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Invalid verification token." }, HttpStatusCode.BadRequest);
            if (String.IsNullOrEmpty(url))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Missing relative page url." }, HttpStatusCode.BadRequest);
            else if (!url.StartsWith("/"))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Url must start with a \"/\"." }, HttpStatusCode.BadRequest);

            HttpResponse.RemoveOutputCacheItem(url);
            return base.Json(new PageViewModel { Success = true });
        }

        //internal IHttpHandler Serialize(string key, bool prefix = false)
        //{
        //    //TODO: Custom serializer routing, we want to serialize the data we can serialize, instead of failing on the first bad object
        //    var cache = new HttpCacheShim() as ICache;
        //    var model = new SerializeViewModel(cache, key, prefix) { Success = true };
        //    return base.Json(model);
        //}

        //internal IHttpHandler Settings()
        //{
        //    var model = new SettingsViewModel() { Success = true };
        //    return base.Json(model);
        //}

        //internal IHttpHandler Stats()
        //{
        //    var cache = new HttpCacheShim() as ICache;
        //    var model = new StatsViewModel(cache, HttpContext.Current) { Success = true };
        //    return base.Json(model);
        //}
    }
}