@requires:nelim.adaptivestorageneolithic
Feature: a disabled alias stays disabled after the game is closed and reopened

  # The claim this pair exists for: Runtime.Choices is fixed for a whole load, so a change taking effect
  # can only be shown by a second process reading what the first one wrote. The first step is the guard:
  # it refuses to pass when the writer ran in this same process (a restart test that never restarted).

  Scenario: the previous launch's choices were loaded, and the guarded content and the ambiguity follow them
    Given Resolve This Instead the settings kept by the previous launch are in place
    Then Resolve This Instead the alias from "adaptive.storage.neolithic" to "nelim.adaptivestorageneolithic" was loaded as off
    And no def "RTI_TestMayRequireDeclared" exists
    And no def "RTI_TestNameGuardDeclared" exists
    And Resolve This Instead the pair from "nelim.resolvethisinstead.test.ambiguous" resolved to "Mlie.UseThisInstead" after the previous launch's pick
    And no errors were logged
