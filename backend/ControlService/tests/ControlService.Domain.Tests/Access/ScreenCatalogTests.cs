using ControlService.Domain.Access;

namespace ControlService.Domain.Tests.Access;

public class ScreenCatalogTests
{
    // #10-T04
    [Fact]
    public void Catalog_has_the_same_screen_keys_as_ScreenKeys_in_the_same_order() // docs/product/screen-catalog.json
    {
        var catalogKeys = ScreenCatalog.Areas.SelectMany(area => area.Screens).Select(screen => screen.Key);

        catalogKeys.ShouldBe(ScreenKeys.All);
    }
}
