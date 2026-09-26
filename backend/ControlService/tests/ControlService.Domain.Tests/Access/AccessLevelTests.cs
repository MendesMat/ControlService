using ControlService.Domain.Access;

namespace ControlService.Domain.Tests.Access;

public class AccessLevelTests
{
    [Fact]
    public void AccessLevel_wire_values_match_negado_leitor_editor_gerenciador() // PERM-02
    {
        AccessLevel.Denied.ToWireValue().ShouldBe("negado");
        AccessLevel.Reader.ToWireValue().ShouldBe("leitor");
        AccessLevel.Editor.ToWireValue().ShouldBe("editor");
        AccessLevel.Manager.ToWireValue().ShouldBe("gerenciador");
    }

    [Fact]
    public void AccessLevel_denied_is_lower_than_every_other_level() // PERM-02
    {
        (AccessLevel.Denied < AccessLevel.Reader).ShouldBeTrue();
        (AccessLevel.Denied < AccessLevel.Editor).ShouldBeTrue();
        (AccessLevel.Denied < AccessLevel.Manager).ShouldBeTrue();
    }

    [Fact]
    public void AccessLevel_manager_is_higher_than_every_other_level() // PERM-02
    {
        (AccessLevel.Manager > AccessLevel.Denied).ShouldBeTrue();
        (AccessLevel.Manager > AccessLevel.Reader).ShouldBeTrue();
        (AccessLevel.Manager > AccessLevel.Editor).ShouldBeTrue();
    }
}
