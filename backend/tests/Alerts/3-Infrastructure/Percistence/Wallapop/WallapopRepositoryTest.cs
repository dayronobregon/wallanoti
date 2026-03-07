using System.Net;
using System.Text;
using System.Web;
using Wallanoti.Src.Alerts.Infrastructure.Percistence.Wallapop;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Alerts._3_Infrastructure.Percistence.Wallapop;

public class WallapopRepositoryTest
{
    [Fact]
    public async Task Latest_AlwaysUsesSectionEndpointAndForcesNewestToday()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                    {
                      "data": {
                        "section": {
                          "items": []
                        }
                      }
                    }
                    """)
            });
        var sut = new WallapopRepository(new HttpClient(handler));

        await sut.Latest(Url.Create("https://es.wallapop.com/app/search?keywords=iphone&order_by=closest"));

        var request = Assert.Single(handler.Requests);
        Assert.NotNull(request.RequestUri);
        Assert.Equal("/api/v3/search/section", request.RequestUri!.AbsolutePath);

        var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
        Assert.Equal("iphone", query["keywords"]);
        Assert.Equal("today", query["time_filter"]);
        Assert.Equal("newest", query["order_by"]);
        Assert.Equal("organic_search_results", query["section_type"]);
    }

    [Fact]
    public async Task Latest_ParsesItemsFromSectionItems()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                    {
                      "data": {
                        "section": {
                          "items": [
                            {
                              "id": "item-1",
                              "user_id": "user-1",
                              "title": "iPhone",
                              "description": "like new",
                              "category_id": 1,
                              "price": { "amount": 500, "currency": "EUR" },
                              "images": [
                                { "averageColor": "#fff", "urls": { "small": "s", "medium": "m", "big": "b" } }
                              ],
                              "reserved": { "flag": false },
                              "location": {
                                "latitude": 0,
                                "longitude": 0,
                                "postal_code": "08001",
                                "city": "Barcelona",
                                "region": "Catalunya",
                                "region2": "",
                                "country_code": "ES"
                              },
                              "shipping": {
                                "shipping_price": true,
                                "user_allows_shipping": true,
                                "cost_configuration_id": "1"
                              },
                              "favorited": { "flag": false },
                              "bump": { "type": "none" },
                              "web_slug": "iphone-slug",
                              "created_at": 1,
                              "modified_at": 2,
                              "previous_price": { "amount": 550, "currency": "EUR" }
                            }
                          ]
                        }
                      }
                    }
                    """)
            });
        var sut = new WallapopRepository(new HttpClient(handler));

        var items = await sut.Latest(Url.Create("https://es.wallapop.com/app/search?keywords=iphone"));

        var item = Assert.Single(items!);
        Assert.Equal("item-1", item.Id);
        Assert.NotNull(item.Price);
        Assert.Equal(500, item.Price!.CurrentPrice);
        Assert.Equal(550, item.Price.PreviousPrice);
    }

    [Fact]
    public async Task Latest_ParsesItemsFromLegacyPayloadItems()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                    {
                      "data": {
                        "section": {
                          "payload": {
                            "items": [
                              {
                                "id": "item-legacy",
                                "user_id": "user-legacy",
                                "title": "legacy",
                                "description": "legacy item",
                                "category_id": 1,
                                "price": { "amount": 100, "currency": "EUR" },
                                "images": [],
                                "reserved": { "flag": false },
                                "location": {
                                  "latitude": 0,
                                  "longitude": 0,
                                  "postal_code": "08001",
                                  "city": "Barcelona",
                                  "region": "Catalunya",
                                  "region2": "",
                                  "country_code": "ES"
                                },
                                "shipping": {
                                  "shipping_price": false,
                                  "user_allows_shipping": true,
                                  "cost_configuration_id": "1"
                                },
                                "favorited": { "flag": false },
                                "bump": { "type": "none" },
                                "web_slug": "legacy-slug",
                                "created_at": 1,
                                "modified_at": 1,
                                "discount": { "previous_price": { "amount": 120, "currency": "EUR" }, "percentage": 10 }
                              }
                            ]
                          }
                        }
                      }
                    }
                    """)
            });
        var sut = new WallapopRepository(new HttpClient(handler));

        var items = await sut.Latest(Url.Create("https://es.wallapop.com/app/search?keywords=legacy"));

        var item = Assert.Single(items!);
        Assert.Equal("item-legacy", item.Id);
        Assert.NotNull(item.Price);
        Assert.Equal(100, item.Price!.CurrentPrice);
        Assert.Equal(120, item.Price.PreviousPrice);
    }

    [Fact]
    public async Task Latest_WhenResponseHasDataButMissingSection_ShouldThrow()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                    {
                      "data": {}
                    }
                    """)
            });
        var sut = new WallapopRepository(new HttpClient(handler));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Latest(Url.Create("https://es.wallapop.com/app/search?keywords=iphone")));
    }

    [Fact]
    public async Task Latest_WhenPriceIsMissing_ShouldKeepItemPriceNull()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                    {
                      "data": {
                        "section": {
                          "items": [
                            {
                              "id": "item-no-price",
                              "user_id": "user-1",
                              "title": "No price",
                              "description": "without price",
                              "category_id": 1,
                              "images": [],
                              "reserved": { "flag": false },
                              "location": {
                                "latitude": 0,
                                "longitude": 0,
                                "postal_code": "08001",
                                "city": "Barcelona",
                                "region": "Catalunya",
                                "region2": "",
                                "country_code": "ES"
                              },
                              "shipping": {
                                "shipping_price": false,
                                "user_allows_shipping": true,
                                "cost_configuration_id": "1"
                              },
                              "favorited": { "flag": false },
                              "bump": { "type": "none" },
                              "web_slug": "no-price-slug",
                              "created_at": 1,
                              "modified_at": 1
                            }
                          ]
                        }
                      }
                    }
                    """)
            });
        var sut = new WallapopRepository(new HttpClient(handler));

        var items = await sut.Latest(Url.Create("https://es.wallapop.com/app/search?keywords=noprice"));

        var item = Assert.Single(items!);
        Assert.Null(item.Price);
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;
        public List<HttpRequestMessage> Requests { get; } = new();

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responseFactory(request));
        }
    }
}
