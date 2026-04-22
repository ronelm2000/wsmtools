using System.Text.RegularExpressions;

namespace Montage.Weiss.Tools.Entities.External.DeckLog;

public class DeckLogSettings
{
    public CardLanguage Language { get; set; } = CardLanguage.Japanese;
    public CardSide Side { get; set; } = CardSide.Both;
    public string Version { get; set; } = "20250609.001";
    public string Authority { get; set; } = "https://decklog.bushiroad.com/";
    public Regex DeckURLMatcher { get; set; } = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)");
    public string VersionURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/version/";
    public string ImagePrefix { get; set; } = "https://ws-tcg.com/wordpress/wp-content/images/cardlist/";
    public string Referrer { get; set; } = "https://decklog.bushiroad.com/create?c=2";
    public string SearchURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/search/2";
    public string CardParamURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/cardparam/2";
    public string SuggestURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/suggest/2";
    public string DeckViewURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/view/";
    public string DeckPublishURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/publish/2";
    public string DeckCheckURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/check/2";
    public string CreateURL { get; set; } = "https://decklog.bushiroad.com/system/app/api/create/";
    public string DeckFriendlyViewURL { get; set; } = "https://decklog.bushiroad.com/view/";

    public static DeckLogSettings Japanese => new();
    public static DeckLogSettings JapaneseInEN => new DeckLogSettings
    {
        Language = CardLanguage.Japanese,
        Side = CardSide.Both,
        Version = "20251118.001",
        Authority = "https://decklog-en.bushiroad.com/",
        DeckURLMatcher = new Regex(@"(.*):\/\/decklog-en\.bushiroad\.com\/ja\/view\/([^\?]*)(.*)"),
        SuggestURL = "https://decklog-en.bushiroad.com/system/app-ja/api/suggest/102",
        DeckViewURL = "https://decklog-en.bushiroad.com/system/app-ja/api/view/",
        DeckPublishURL = "https://decklog-en.bushiroad.com/system/app-ja/api/publish/102",
        DeckCheckURL = "https://decklog-en.bushiroad.com/system/app-ja/api/check/102",
        ImagePrefix = "https://ws-tcg.com/wordpress/wp-content/images/cardlist/",
        Referrer = "https://decklog-en.bushiroad.com/ja/create?c=102",
        SearchURL = "https://decklog-en.bushiroad.com/system/app-ja/api/search/102",
        CardParamURL = "https://decklog-en.bushiroad.com/system/app-ja/api/cardparam/102",
        VersionURL = "https://decklog-en.bushiroad.com/system/app-ja/api/version/",
        CreateURL = "https://decklog-en.bushiroad.com/system/app-ja/api/create/",
        DeckFriendlyViewURL = "https://decklog-en.bushiroad.com/ja/view/"
    };
    public static DeckLogSettings English => new DeckLogSettings
    {
        Language = CardLanguage.English,
        Side = CardSide.Both,
        Version = "20251118.001",
        Authority = "https://decklog-en.bushiroad.com/",
        DeckURLMatcher = new Regex(@"(.*):\/\/decklog-en\.bushiroad\.com\/view\/([^\?]*)(.*)"),
        SuggestURL = "https://decklog-en.bushiroad.com/system/app/api/suggest/2",
        DeckViewURL = "https://decklog-en.bushiroad.com/system/app/api/view/",
        DeckPublishURL = "https://decklog-en.bushiroad.com/system/app/api/publish/2",
        DeckCheckURL = "https://decklog-en.bushiroad.com/system/app/api/check/2",
        ImagePrefix = "https://en.ws-tcg.com/wordpress/wp-content/images/cardimages/",
        Referrer = "https://decklog-en.bushiroad.com/create?c=2",
        SearchURL = "https://decklog-en.bushiroad.com/system/app/api/search/2",
        CardParamURL = "https://decklog-en.bushiroad.com/system/app/api/cardparam/2",
        VersionURL = "https://decklog-en.bushiroad.com/system/app/api/version/",
        CreateURL = "https://decklog-en.bushiroad.com/system/app/api/create/",
        DeckFriendlyViewURL = "https://decklog-en.bushiroad.com/view/"
    };
    public static DeckLogSettings Rose => new DeckLogSettings
    {
        Language = CardLanguage.Japanese,
        Side = CardSide.Rose,
        Version = "20251118.001",
        Authority = "https://decklog.bushiroad.com/",
        DeckURLMatcher = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)"),
        SuggestURL = "https://decklog.bushiroad.com/system/app/api/suggest/12",
        DeckViewURL = "https://decklog.bushiroad.com/system/app/api/view/",
        DeckPublishURL = "https://decklog.bushiroad.com/system/app/api/publish/12",
        DeckCheckURL = "https://decklog.bushiroad.com/system/app/api/check/12",
        ImagePrefix = "https://ws-rose.com/wordpress/wp-content/images/cardlist/",
        Referrer = "https://decklog.bushiroad.com/create?c=12",
        SearchURL = "https://decklog.bushiroad.com/system/app/api/search/12",
        VersionURL = "https://decklog.bushiroad.com/system/app/api/version/",
        CreateURL = "https://decklog.bushiroad.com/system/app/api/create/",
        DeckFriendlyViewURL = "https://decklog.bushiroad.com/view/"
    };
    public static DeckLogSettings RoseInEN => new DeckLogSettings
    {
        Language = CardLanguage.Japanese,
        Side = CardSide.Rose,
        Version = "20251118.001",
        Authority = "https://decklog-en.bushiroad.com/",
        DeckURLMatcher = new Regex(@"(.*):\/\/decklog-en\.bushiroad\.com\/ja/view\/([^\?]*)(.*)"),
        SuggestURL = "https://decklog-en.bushiroad.com/system/app-ja/api/suggest/110",
        DeckViewURL = "https://decklog-en.bushiroad.com/system/app-ja/api/view/",
        DeckPublishURL = "https://decklog-en.bushiroad.com/system/app-ja/api/publish/110",
        DeckCheckURL = "https://decklog-en.bushiroad.com/system/app-ja/api/check/110",
        ImagePrefix = "https://ws-rose.com/wordpress/wp-content/images/cardlist/",
        Referrer = "https://decklog-en.bushiroad.com/ja/create?c=110",
        SearchURL = "https://decklog-en.bushiroad.com/system/app-ja/api/search/110",
        VersionURL = "https://decklog-en.bushiroad.com/system/app-ja/api/version/",
        CreateURL = "https://decklog-en.bushiroad.com/system/app-ja/api/create/",
        DeckFriendlyViewURL = "https://decklog-en.bushiroad.com/ja/view/"
    };
}
