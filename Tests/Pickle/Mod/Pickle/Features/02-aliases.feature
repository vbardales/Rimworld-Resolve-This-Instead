@requires:nelim.adaptivestorageneolithic
Feature: A declared replacement stands in for the missing mod, a proposed one does not

  # Pass avec-neolithic-nps. The companion embeds About/ResolveThisInstead.xml vouching for adaptive.storage.neolithic ->
  # nelim.adaptivestorageneolithic; the original module is NOT staged. Nature's Pretty Sweet (Continued) is staged, and its
  # old page is only a proposal of Use This Instead's data: it must stay off until the player switches it on.

  Scenario: the original module is absent and its replacement is loaded
    Then mod "nelim.adaptivestorageneolithic" is loaded
    And mod "adaptive.storage.neolithic" is not loaded

  Scenario: a declared pair satisfies a MayRequire on the original's package id
    Then def "RTI_TestMayRequireDeclared" of type "StatCategoryDef" exists

  Scenario: a declared pair satisfies a name guard on the original's display name
    Then def "RTI_TestNameGuardDeclared" of type "StatCategoryDef" exists

  Scenario: once the game has started, the original module is not treated as loaded any more
    Then Resolve This Instead: the game does not treat "adaptive.storage.neolithic" as loaded
    And Resolve This Instead: the game does not treat the mod named "Adaptive Storage Neolithic Module" as loaded

  Scenario: a mod C that asks for the original in its constructor sees it as absent, and no options entry exists for it
    Then Resolve This Instead: the consumer mod's constructor saw "adaptive.storage.neolithic" as absent
    And Resolve This Instead: the mod options list has no entry for "adaptive.storage.neolithic"

  Scenario: the mod list's incompatibleWith warning of the Renew names nothing
    Then Resolve This Instead: the incompatibleWith of "nelim.adaptivestorageneolithic" names no loaded mod

  @requires:nelim.resolvethisinstead.testconsumer
  Scenario: the generic runtime mechanism discriminates by caller, not by rule
    # Same pair, two callers: this suite's own step (no runtime opt-in) must still see reality; the separately staged consumer C, whose
    # OWN embedded file asks for <runtime>true</runtime>, must see the alias. Proves the design settled after finding that "any mod gets
    # it" would have fooled a diagnostic mod exactly like it would have fooled the engine.
    Then Resolve This Instead: an untrusted mod's own call to IsActive still sees "adaptive.storage.neolithic" as absent
    When Resolve This Instead: the test consumer C checks "adaptive.storage.neolithic" from its own runtime code
    Then Resolve This Instead: the test consumer C's own runtime code saw "adaptive.storage.neolithic" as present

  Scenario: the controls stay absent
    Then no def "RTI_TestMayRequireControl" exists

  Scenario: no error came from the aliases
    Then no errors were logged
