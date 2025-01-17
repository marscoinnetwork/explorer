using System;
using System.Globalization;
using Martiscoin.Explorer.Settings;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Martiscoin.Explorer.TagHelpers
{
   public class CoinTagHelper : TagHelper
   {
      private readonly ExplorerSettings settings;

      private readonly ChainSettings chainSettings;

      private readonly ILogger<CoinTagHelper> log;

      [HtmlAttributeName("Positive")]
      public bool? Positive { get; set; }

      public CoinTagHelper(IOptions<ExplorerSettings> settings, IOptions<ChainSettings> chainSettings, ILogger<CoinTagHelper> log)
      {
         this.settings = settings.Value;
         this.chainSettings = chainSettings.Value;
         this.log = log;
      }

      public override void Process(TagHelperContext context, TagHelperOutput output)
      {
         string input = output.GetChildContentAsync().Result.GetContent();
         bool success = long.TryParse(input, out long value);

         string cssExtra = string.Empty;

         if (Positive.HasValue)
         {
            cssExtra = Positive.Value ? "coin-value-positive" : "coin-value-negative";
         }

         if (success)
         {
            try
            {
               string[] values = (value / 100000000d).ToString("N8").Split(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
               string html = $"<span class=\"coin-value-upper {cssExtra}\">{values[0]}</span><span class=\"coin-value-lower {cssExtra}\">{NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator}{values[1]}</span> <span class=\"coin-value-tag {cssExtra}\">{chainSettings.Symbol}</span>";
               output.Content.SetHtmlContent(html);
            }
            catch (Exception ex)
            {
               log.LogError(ex, $"Failed to parse in CoinTagHelper. Input was {input} and parsed value was {value}.");
               output.Content.SetContent(input);
            }
         }
         else
         {
            output.Content.SetContent(input);
         }
      }
   }
}
