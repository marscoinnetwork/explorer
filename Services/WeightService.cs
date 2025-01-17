using Martiscoin.Explorer.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Martiscoin.Explorer.Services
{
   public class WeightService : ServiceBase
   {
      private readonly IMemoryCache memoryCache;
      private readonly NetworkSettings settings;

      public WeightService() : base(string.Empty)
      {

      }

      public WeightService(IMemoryCache memoryCache, IOptions<NetworkSettings> settings) : base(settings.Value.APIUrl)
      {
         this.memoryCache = memoryCache;
         this.settings = settings.Value;
      }

      public string DownloadStakingInfo()
      {
         var client = new RestClient($"{settings.APIUrl}:{settings.APIPort}/api/Staking/getstakinginfo");
         var request = new RestRequest(Method.GET);
         IRestResponse result = client.Execute(request);
         return result.Content;
      }


      public string GetNetworkWeight()
      {
         decimal networkWeightResult = 0;
         string cachedStakingInfoResult = memoryCache.Get<string>("StakingInfo"); // Responsibility of caching is put on DataUpdateService.

         if (!string.IsNullOrWhiteSpace(cachedStakingInfoResult))
         {
            var json = JObject.Parse(cachedStakingInfoResult);
            JToken isEnabled = json.SelectToken("enabled");
            if ((bool)isEnabled)
            {
               JToken networkWeightJSON = json.SelectToken("netStakeWeight");
               decimal networkWeight = Convert.ToDecimal(networkWeightJSON);
               networkWeightResult = networkWeight;
            }
            else
            {
               const string message = "Node is not staking, turn on staking to retrieve the staking weight.";
               var blockIndexServiceException = new ApplicationException(message);
               throw blockIndexServiceException;
            }
         }

         return string.Format("{0:n0}", networkWeightResult);
      }
   }
}
