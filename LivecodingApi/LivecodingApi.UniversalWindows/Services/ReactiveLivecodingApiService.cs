﻿using LivecodingApi.Configuration;
using LivecodingApi.Model;
using LivecodingApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if __IOS__ || __ANDROID__ || NET45
using System.Net.Http;
using System.Net.Http.Headers;
#endif
#if NETFX_CORE
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
using Windows.Security.Authentication.Web;
#endif
using System.Reactive.Threading.Tasks;

namespace LivecodingApi.Services
{
    public class ReactiveLivecodingApiService : IReactiveLivecodingApiService
    {
        #region Fields

        private readonly string _baseApiAddress = $"{Constants.ApiBaseUrl}{Constants.ApiVersion}";

        private HttpClient HttpClient
        {
            get
            {
                var httpClient = new HttpClient();

#if __IOS__ || __ANDROID__ || NET45
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(Token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
#endif
#if NETFX_CORE
                httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(Token))
                    httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", Token);
#endif

                return httpClient;
            }
        }

        private static string[] _scopes = new string[]
        {
            "read",
            "read:viewer",
            "read:user",
            "read:channel",
            "chat"
        };

        #endregion

        #region Properties

        public string Token { get; set; }

        #endregion

        #region Constructors

        public ReactiveLivecodingApiService() { }

        public ReactiveLivecodingApiService(string token)
        {
            Token = token;
        }

        #endregion

        #region Authentication

        public IObservable<bool?> Login(string oauthKey, string oauthSecret)
        {
#if NETFX_CORE
            return Task.Run<bool?>(async () =>
            {
                try
                {
                    var state = Guid.NewGuid();
                    string scopes = string.Join(" ", _scopes);

                    string startUrl = $"https://www.livecoding.tv/o/authorize?scope={scopes}&state={state}&redirect_uri={AuthHelper.RedirectUrl}&response_type=token&client_id={oauthKey}";
                    var startUri = new Uri(startUrl);
                    var endUri = new Uri(AuthHelper.RedirectUrl);

                    var webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                    Token = AuthHelper.RetrieveToken(webAuthenticationResult, oauthKey, oauthSecret);
                    return !string.IsNullOrWhiteSpace(Token);
                }
                catch
                {
                    return null;
                }
            }).ToObservable();
#else
            throw new NotImplementedException();
#endif
        }

        #endregion

        #region Coding Categories

        public IObservable<PaginationResult<CodingCategory>> GetCodingCategories(PaginationRequest request)
        {
            string url = _baseApiAddress + "codingcategories/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<CodingCategory>>(url)
                .ToObservable();
        }

        public IObservable<CodingCategory> GetCodingCategoryByName(string name)
        {
            string url = _baseApiAddress + $"codingcategories/{name}/";
            return HttpClient.GetAsync<CodingCategory>(url)
                .ToObservable();
        }

        #endregion

        #region Livestreams

        public IObservable<PaginationResult<LiveStream>> GetLiveStreams(PaginationRequest request)
        {
            string url = _baseApiAddress + "livestreams/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<LiveStream>>(url)
                .ToObservable();
        }

        public IObservable<PaginationResult<LiveStream>> GetLiveStreamsOnAir(PaginationRequest request)
        {
            string url = _baseApiAddress + "livestreams/onair/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<LiveStream>>(url)
                .ToObservable();
        }

        public IObservable<LiveStream> GetCurrentLivestreamOfUser(string userSlug)
        {
            string url = _baseApiAddress + $"livestreams/{userSlug}/";
            return HttpClient.GetAsync<LiveStream>(url)
                .ToObservable();
        }

        #endregion

        #region Languages

        public IObservable<PaginationResult<SiteLanguage>> GetLanguages(PaginationRequest request)
        {
            string url = _baseApiAddress + "languages/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<SiteLanguage>>(url)
                .ToObservable();
        }

        public IObservable<SiteLanguage> GetLanguageByIsoCode(string iso)
        {
            string url = _baseApiAddress + $"languages/{iso}/";
            return HttpClient.GetAsync<SiteLanguage>(url)
                .ToObservable();
        }

        #endregion

        #region Scheduled Broadcast

        public IObservable<PaginationResult<ScheduledBroadcast>> GetScheduledBroadcasts(PaginationRequest request)
        {
            string url = _baseApiAddress + "scheduledbroadcast/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<ScheduledBroadcast>>(url)
                .ToObservable();
        }

        public IObservable<ScheduledBroadcast> GetScheduledBroadcastById(string id)
        {
            string url = _baseApiAddress + $"scheduledbroadcast/{id}/";
            return HttpClient.GetAsync<ScheduledBroadcast>(url)
                .ToObservable();
        }

        #endregion

        #region Videos

        public IObservable<PaginationResult<Video>> GetVideos(PaginationRequest request)
        {
            string url = _baseApiAddress + "videos/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<Video>>(url)
                .ToObservable();
        }

        public IObservable<Video> GetVideoBySlug(string videoSlug)
        {
            string url = _baseApiAddress + $"videos/{videoSlug}/";
            return HttpClient.GetAsync<Video>(url)
                .ToObservable();
        }

        #endregion

        #region User

        public IObservable<UserPrivate> GetCurrentUser()
        {
            string url = _baseApiAddress + "user/";
            return HttpClient.GetAsync<UserPrivate>(url)
                .ToObservable();
        }

        public IObservable<IEnumerable<User>> GetFollowers()
        {
            string url = _baseApiAddress + "user/followers/";
            return HttpClient.GetAsync<IEnumerable<User>>(url)
                .ToObservable();
        }

        public IObservable<IEnumerable<User>> GetFollows()
        {
            string url = _baseApiAddress + "user/follows/";
            return HttpClient.GetAsync<IEnumerable<User>>(url)
                .ToObservable();
        }

        public IObservable<XmppAccount> GetXmppAccount()
        {
            string url = _baseApiAddress + "user/chat/account/";
            return HttpClient.GetAsync<XmppAccount>(url)
                .ToObservable();
        }

        public IObservable<PaginationResult<LiveStreamPrivate>> GetUserLivestreams(PaginationRequest request)
        {
            string url = _baseApiAddress + "user/livestreams/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<LiveStreamPrivate>>(url)
                .ToObservable();
        }

        public IObservable<PaginationResult<LiveStreamPrivate>> GetUserLivestreamsOnAir(PaginationRequest request)
        {
            string url = _baseApiAddress + "user/livestreams/onair/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<LiveStreamPrivate>>(url)
                .ToObservable();
        }

        public IObservable<PaginationResult<Video>> GetUserVideos(PaginationRequest request)
        {
            string url = _baseApiAddress + "user/videos/";
            url += PaginationRequestHelper.CreateHttpQueryParams(request);
            return HttpClient.GetAsync<PaginationResult<Video>>(url)
                .ToObservable();
        }

        public IObservable<IEnumerable<Video>> GetUserLatestVideos()
        {
            string url = _baseApiAddress + "user/videos/latest/";
            return HttpClient.GetAsync<IEnumerable<Video>>(url)
                .ToObservable();
        }

        #endregion
    }
}
