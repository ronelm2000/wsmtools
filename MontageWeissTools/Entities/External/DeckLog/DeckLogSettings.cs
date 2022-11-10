using System.Text.RegularExpressions;

namespace Montage.Weiss.Tools.Entities.External.DeckLog;

public class DeckLogSettings
{
    public CardLanguage Language { get; set; } = CardLanguage.Japanese;
    public string Version { get; set; } = "20221013.001";
    public string Authority { get; set; } = "https://decklog.bushiroad.com/";
    public Regex DeckURLMatcher { get; set; } = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)");
    public string VersionURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/version/";
    public string ImagePrefix { get; set; } = "https://ws-tcg.com/wordpress/wp-content/images/cardlist/";
    //            public string ImagePrefix { get; set; } = "https://s3-ap-northeast-1.amazonaws.com/static.ws-tcg.com/wordpress/wp-content/cardimages/";

    public string Referrer { get; set; } = "https://decklog.bushiroad.com/create?c=2";
    public string SearchURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/search/2";
    public string CardParamURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/cardparam/2";
    public string DeckViewURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/view/";
    public string DeckPublishURL { get; set; } = "https://decklog-.bushiroad.com/system/app/api/publish/2";

    public static DeckLogSettings Japanese => new();
    public static DeckLogSettings English => new DeckLogSettings
    {
        Language = CardLanguage.English,
        Version = "20220713.001",
        Authority = "https://decklog-en.bushiroad.com/",
        DeckURLMatcher = new Regex(@"(.*):\/\/decklog-en\.bushiroad\.com\/view\/([^\?]*)(.*)"),
        CardParamURL = "https://decklog-en.bushiroad.com/system/app/api/cardparam/2",
        DeckViewURL = "https://decklog-en.bushiroad.com/system/app/api/view/",
        DeckPublishURL = "https://decklog-en.bushiroad.com/system/app/api/publish/2",
        ImagePrefix = "https://en.ws-tcg.com/wp/wp-content/images/cardimages/",
        Referrer = "https://decklog-en.bushiroad.com/create?c=2",
        SearchURL = "https://decklog-en.bushiroad.com/system/app/api/search/2",
        VersionURL = "https://decklog-en.bushiroad.com/system/app/api/version/"
    };
}
